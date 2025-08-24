using AutoMapper;
using AutoMapper.QueryableExtensions;
using Castle.Core.Resource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Narije.Core.DTOs.Admin;
using Narije.Core.DTOs.Enum;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Export;
using Narije.Core.DTOs.ViewModels.Recipts;
using Narije.Core.DTOs.ViewModels.Reserve;
using Narije.Core.DTOs.ViewModels.Search;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;

namespace Narije.Infrastructure.Repositories
{
    public class ReciptsRepository : BaseRepository<Recipt>, IRecipts
    {
        public ReciptsRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
         base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }
    

       #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Recipt");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Recipt
                        .ProjectTo<ReciptResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Searchs = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Searchs.Data, _Meta: Searchs.Meta, _Header: header);

        }
        #endregion


        #region ExportRecipt

        public async Task<FileContentResult> ExportRecipt(int customerId, DateTime date)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/ReciptTemplate.xlsx");
            using var package = new ExcelPackage(new FileInfo(templatePath));
            var ws = package.Workbook.Worksheets[0];

            var customer = await _NarijeDBContext.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer == null) throw new Exception("Customer not found");

            string parentTitle = null;
            if (customer.ParentId.HasValue)
            {
                parentTitle = await _NarijeDBContext.Customers
                    .Where(p => p.Id == customer.ParentId.Value)
                    .Select(p => p.Title)
                    .FirstOrDefaultAsync();
            }

            var customerFullTitle = string.IsNullOrWhiteSpace(parentTitle)
                ? customer.Title
                : $"{customer.Title} - {parentTitle}";

            ws.Cells["B2"].Value = customerFullTitle;                          // نام مشتری
            ws.Cells["D2"].Value = customer.DeliverFullName ?? string.Empty;   // نام تحویل گیرنده
            ws.Cells["G2"].Value = customer.Address ?? string.Empty;           // آدرس
            ws.Cells["B3"].Value = customer.Code ?? string.Empty;              // کد مشتری
            ws.Cells["D3"].Value = customer.DeliverPhoneNumber ?? string.Empty;// شماره تماس تحویل گیرنده
            ws.Cells["G3"].Value = date.ToString("yyyy/MM/dd");               // تاریخ

            var reserves = await _NarijeDBContext.vReserves
                .Where(r => r.CustomerId == customerId && r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict)
                .GroupBy(r => new { r.FoodId, r.FoodTitle, r.FoodArpaNumber })
                .Select(g => new
                {
                    FoodCode = g.Key.FoodArpaNumber ?? g.Key.FoodId.ToString(),
                    FoodTitle = g.Key.FoodTitle,
                    Quantity = g.Sum(x => x.Num)
                })
                .OrderBy(x => x.FoodTitle)
                .ToListAsync();
            int startRow = 6;
            int templateRowCount = 6;
            int nextSectionRow = 12;

            if (reserves.Count > templateRowCount)
            {
                int extraRows = reserves.Count - templateRowCount;
                ws.InsertRow(nextSectionRow, extraRows);

                int styleA = ws.Cells[11, 1].StyleID;
                int styleB = ws.Cells[11, 2].StyleID;
                int styleC = ws.Cells[11, 3].StyleID;
                int styleD = ws.Cells[11, 4].StyleID;
                int styleE = ws.Cells[11, 5].StyleID;
                int styleF = ws.Cells[11, 6].StyleID;
                int styleG = ws.Cells[11, 7].StyleID;
                double rowHeight = ws.Row(11).Height;

                for (int r = nextSectionRow; r < nextSectionRow + extraRows; r++)
                {
                    ws.Row(r).Height = rowHeight;
                    ws.Cells[r, 1].StyleID = styleA;
                    ws.Cells[r, 2].StyleID = styleB;
                    ws.Cells[r, 3].StyleID = styleC;
                    ws.Cells[r, 4].StyleID = styleD;
                    ws.Cells[r, 5].StyleID = styleE;
                    ws.Cells[r, 6].StyleID = styleF;
                    ws.Cells[r, 7].StyleID = styleG;

                    ws.Cells[r, 3, r, 4].Merge = true;
                    ws.Cells[r, 6, r, 7].Merge = true;
                }
            }

            int row = startRow;

            foreach (var item in reserves)
            {
                ws.Cells[row, 1].Value = row - startRow + 1;      // ردیف
                ws.Cells[row, 2].Value = item.FoodCode;            // کد کالا
                ws.Cells[row, 3].Value = item.FoodTitle;           // نام کالا (C:D merged)
                ws.Cells[row, 5].Value = item.Quantity;            // مقدار/تعداد
                ws.Cells[row, 6].Value = string.Empty;             // توضیحات (F:G merged)
                row++;
            }

            var excelBytes = package.GetAsByteArray();
            return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");


        }

        #endregion


        #region ExportPdfRecipt
        public async Task<FileContentResult> ExportPdfRecipt(int customerId, DateTime date)
        {
            // Reuse the same data mapping used for Excel
            var customer = await _NarijeDBContext.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer == null) throw new Exception("Customer not found");

            string parentTitle = null;
            if (customer.ParentId.HasValue)
            {
                parentTitle = await _NarijeDBContext.Customers
                    .Where(p => p.Id == customer.ParentId.Value)
                    .Select(p => p.Title)
                    .FirstOrDefaultAsync();
            }
            var customerFullTitle = string.IsNullOrWhiteSpace(parentTitle)
                ? customer.Title
                : $"{customer.Title} - {parentTitle}";

            var reserves = await _NarijeDBContext.vReserves
                .Where(r => r.CustomerId == customerId && r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict && r.IsFood == true)
                .GroupBy(r => new { r.FoodId, r.FoodTitle, r.FoodArpaNumber })
                .Select(g => new
                {
                    FoodCode = g.Key.FoodArpaNumber ?? g.Key.FoodId.ToString(),
                    FoodTitle = g.Key.FoodTitle,
                    Quantity = g.Sum(x => x.Num)
                })
                .OrderBy(x => x.FoodTitle)
                .ToListAsync();

            // Build PDF using QuestPDF
            QuestPDF.Fluent.Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(QuestPDF.Infrastructure.PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Text("رسید تحویل محصول").FontSize(16).SemiBold().AlignCenter();
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text(text => { text.Span("نام مشتری: "); text.Span(customerFullTitle); });
                            r.RelativeItem().Text(text => { text.Span("کد مشتری: "); text.Span(customer.Code ?? ""); });
                        });
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text(text => { text.Span("نام تحویل گیرنده: "); text.Span(customer.DeliverFullName ?? ""); });
                            r.RelativeItem().Text(text => { text.Span("شماره تماس تحویل گیرنده: "); text.Span(customer.DeliverPhoneNumber ?? ""); });
                        });
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text(text => { text.Span("آدرس: "); text.Span(customer.Address ?? ""); });
                            r.RelativeItem().Text(text => { text.Span("تاریخ: "); text.Span(date.ToString("yyyy/MM/dd")); });
                        });

                        col.Item().Text("مشخصات موادغذایی ارسالی").SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);   // ردیف
                                columns.RelativeColumn(2);   // کد کالا
                                columns.RelativeColumn(4);   // نام کالا
                                columns.RelativeColumn(2);   // مقدار/تعداد
                                columns.RelativeColumn(4);   // توضیحات
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("ردیف").SemiBold();
                                header.Cell().Text("کد کالا").SemiBold();
                                header.Cell().Text("نام کالا").SemiBold();
                                header.Cell().Text("مقدار/ تعداد").SemiBold();
                                header.Cell().Text("توضیحات").SemiBold();
                            });

                            int index = 1;
                            foreach (var item in reserves)
                            {
                                table.Cell().Text(index.ToString());
                                table.Cell().Text(item.FoodCode);
                                table.Cell().Text(item.FoodTitle);
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text("");
                                index++;
                            }
                        });
                    });
                });
            })
            .GeneratePdf(out byte[] pdfBytes);

            return new FileContentResult(pdfBytes, "application/pdf")
            {
                FileDownloadName = $"رسید-{customerFullTitle}-{date:yyyyMMdd}.pdf"
            };
        }


        #endregion
    }
}