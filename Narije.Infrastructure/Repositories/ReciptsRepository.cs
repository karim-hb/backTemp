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
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

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
        
        public async Task<FileContentResult> ExportRecipt(int? customerId, DateTime date)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();

            string xlsxPath = null;
            Recipt recipt = null;
            try
            {
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/ReciptTemplate.xlsx");
                using var package = new ExcelPackage(new FileInfo(templatePath));
                var ws = package.Workbook.Worksheets[0];

                // Fetch customer (optional)
                Customer customer = null;
                string parentTitle = null;
                if (customerId.HasValue)
                {
                    customer = await _NarijeDBContext.Customers.FirstOrDefaultAsync(c => c.Id == customerId.Value);
                    if (customer?.ParentId.HasValue == true)
                    {
                        parentTitle = await _NarijeDBContext.Customers
                            .Where(p => p.Id == customer.ParentId.Value)
                            .Select(p => p.Title)
                            .FirstOrDefaultAsync();
                    }
                }

                var customerFullTitle = (customer == null)
                    ? string.Empty
                    : (string.IsNullOrWhiteSpace(parentTitle) ? customer.Title : $"{customer.Title} - {parentTitle}");

                // Map header cells (empty if no customer)
                ws.Cells["B2"].Value = customerFullTitle;                          // نام مشتری
                ws.Cells["D2"].Value = customer?.DeliverFullName ?? string.Empty;   // نام تحویل گیرنده
                ws.Cells["G2"].Value = customer?.Address ?? string.Empty;           // آدرس
                ws.Cells["B3"].Value = customer?.Code ?? string.Empty;              // کد مشتری
                ws.Cells["D3"].Value = customer?.DeliverPhoneNumber ?? string.Empty;// شماره تماس تحویل گیرنده
                ws.Cells["G3"].Value = date.ToString("yyyy/MM/dd");               // تاریخ

                // Query reserves for the given date and optional customer
                var reserveQuery = _NarijeDBContext.vReserves
                    .Where(r => r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict);
                if (customerId.HasValue)
                {
                    reserveQuery = reserveQuery.Where(r => r.CustomerId == customerId.Value);
                }

                var reserveRecords = await reserveQuery
                    .OrderBy(r => r.FoodTitle)
                    .ToListAsync();

                var reserveIds = reserveRecords.Select(r => r.Id).Distinct().ToList();

                var reserves = reserveRecords
                    .GroupBy(r => new { r.FoodId, r.FoodTitle, r.FoodArpaNumber })
                    .Select(g => new
                    {
                        FoodCode = g.Key.FoodArpaNumber ?? g.Key.FoodId.ToString(),
                        FoodTitle = g.Key.FoodTitle,
                        Quantity = g.Sum(x => x.Num)
                    })
                    .OrderBy(x => x.FoodTitle)
                    .ToList();

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

                // Create Recipt record before finalizing to include file name in the sheet
                var identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((identity is null) || (identity.Claims.Count() == 0))
                    throw new Exception("دسترسی ندارید");
                var userId = Int32.Parse(identity.FindFirst("Id").Value);

                int? customerParentId = null;
                if (customer?.ParentId.HasValue == true)
                    customerParentId = customer.ParentId.Value;

                recipt = new Recipt
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CustomerId = customerId,
                    CustomerParentId = customerParentId,
                    ReserveIds = string.Join(",", reserveIds),
                    FileType = (int)EnumFileType.xlsx,
                    FileName = string.Empty
                };

                await _NarijeDBContext.Recipt.AddAsync(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                recipt.FileName = $"SP_{recipt.Id}";
                _NarijeDBContext.Recipt.Update(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                // Put document code into the template header G1
                ws.Cells["G1"].Value = $"کد سند: {recipt.FileName}";

                var excelBytes = package.GetAsByteArray();

                // Persist physical file under /data/recipts
                var basePath = "/data/recipts";
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);
                xlsxPath = Path.Combine(basePath, $"{recipt.FileName}.xlsx");
                await File.WriteAllBytesAsync(xlsxPath, excelBytes);

                await transaction.CommitAsync();

                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{recipt.FileName}.xlsx"
                };
            }
            catch
            {
                try { await transaction.RollbackAsync(); } catch { }

                if (!string.IsNullOrWhiteSpace(xlsxPath) && File.Exists(xlsxPath))
                {
                    try { File.Delete(xlsxPath); } catch { }
                }

                throw;
            }
        }

        #endregion


        #region ExportPdfRecipt
        public async Task<FileContentResult> ExportPdfRecipt(int? customerId, DateTime date)
        {
            using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();

            string pdfPath = null;
            string tempXlsxPath = null;
            Recipt recipt = null;
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/ReciptTemplate.xlsx");
                using var package = new ExcelPackage(new FileInfo(templatePath));
                var ws = package.Workbook.Worksheets[0];

                Customer customer = null;
                string parentTitle = null;
                if (customerId.HasValue)
                {
                    customer = await _NarijeDBContext.Customers.FirstOrDefaultAsync(c => c.Id == customerId.Value);
                    if (customer?.ParentId.HasValue == true)
                    {
                        parentTitle = await _NarijeDBContext.Customers
                            .Where(p => p.Id == customer.ParentId.Value)
                            .Select(p => p.Title)
                            .FirstOrDefaultAsync();
                    }
                }

                var customerFullTitle = (customer == null)
                    ? string.Empty
                    : (string.IsNullOrWhiteSpace(parentTitle) ? customer.Title : $"{customer.Title} - {parentTitle}");

                ws.Cells["B2"].Value = customerFullTitle;
                ws.Cells["D2"].Value = customer?.DeliverFullName ?? string.Empty;
                ws.Cells["G2"].Value = customer?.Address ?? string.Empty;
                ws.Cells["B3"].Value = customer?.Code ?? string.Empty;
                ws.Cells["D3"].Value = customer?.DeliverPhoneNumber ?? string.Empty;
                ws.Cells["G3"].Value = date.ToString("yyyy/MM/dd");

                var reserveQuery = _NarijeDBContext.vReserves
                    .Where(r => r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict);
                if (customerId.HasValue)
                {
                    reserveQuery = reserveQuery.Where(r => r.CustomerId == customerId.Value);
                }

                var reserveRecords = await reserveQuery
                    .OrderBy(r => r.FoodTitle)
                    .ToListAsync();

                var reserveIds = reserveRecords.Select(r => r.Id).Distinct().ToList();

                var reserves = reserveRecords
                    .GroupBy(r => new { r.FoodId, r.FoodTitle, r.FoodArpaNumber })
                    .Select(g => new
                    {
                        FoodCode = g.Key.FoodArpaNumber ?? g.Key.FoodId.ToString(),
                        FoodTitle = g.Key.FoodTitle,
                        Quantity = g.Sum(x => x.Num)
                    })
                    .OrderBy(x => x.FoodTitle)
                    .ToList();

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
                    ws.Cells[row, 1].Value = row - startRow + 1;
                    ws.Cells[row, 2].Value = item.FoodCode;
                    ws.Cells[row, 3].Value = item.FoodTitle;
                    ws.Cells[row, 5].Value = item.Quantity;
                    ws.Cells[row, 6].Value = string.Empty;
                    row++;
                }

                var identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((identity is null) || (identity.Claims.Count() == 0))
                    throw new Exception("دسترسی ندارید");
                var userId = Int32.Parse(identity.FindFirst("Id").Value);

                int? customerParentId = null;
                if (customer?.ParentId.HasValue == true)
                    customerParentId = customer.ParentId.Value;

                recipt = new Recipt
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CustomerId = customerId,
                    CustomerParentId = customerParentId,
                    ReserveIds = string.Join(",", reserveIds),
                    FileType = (int)EnumFileType.pdf,
                    FileName = string.Empty
                };

                await _NarijeDBContext.Recipt.AddAsync(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                recipt.FileName = $"SP_{recipt.Id}";
                _NarijeDBContext.Recipt.Update(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                ws.Cells["G1"].Value = $"کد سند: {recipt.FileName}";

                var basePath = "/data/recipts";
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);
                tempXlsxPath = Path.Combine(basePath, $"{recipt.FileName}.xlsx");
                await File.WriteAllBytesAsync(tempXlsxPath, package.GetAsByteArray());

                // Convert XLSX to PDF using headless LibreOffice (so the visual output matches Excel template)
                var tempOutputDir = Path.GetDirectoryName(tempXlsxPath);
                var libreOfficePathCandidates = new[] { "/usr/bin/soffice", "/usr/lib/libreoffice/program/soffice" };
                var sofficePath = libreOfficePathCandidates.FirstOrDefault(File.Exists);
                if (string.IsNullOrEmpty(sofficePath))
                    throw new Exception("LibreOffice (soffice) is not installed in the runtime environment.");

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = sofficePath;
                process.StartInfo.Arguments = $"--headless --nologo --convert-to pdf --outdir \"{tempOutputDir}\" \"{tempXlsxPath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                string stdOut = await process.StandardOutput.ReadToEndAsync();
                string stdErr = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new Exception($"LibreOffice conversion failed. ExitCode={process.ExitCode}. Error={stdErr}. Output={stdOut}");
                }

                pdfPath = Path.Combine(basePath, $"{recipt.FileName}.pdf");
                if (!File.Exists(pdfPath))
                {
                    throw new Exception("Expected PDF not found after conversion.");
                }

                // Remove temporary xlsx file. We only keep PDF as the persisted artifact
                try { if (File.Exists(tempXlsxPath)) File.Delete(tempXlsxPath); } catch { }

                await transaction.CommitAsync();

                var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
                return new FileContentResult(pdfBytes, "application/pdf")
                {
                    FileDownloadName = $"{recipt.FileName}.pdf"
                };
            }
            catch
            {
                try { await transaction.RollbackAsync(); } catch { }

                if (!string.IsNullOrWhiteSpace(pdfPath) && File.Exists(pdfPath))
                {
                    try { File.Delete(pdfPath); } catch { }
                }

                if (!string.IsNullOrWhiteSpace(tempXlsxPath) && File.Exists(tempXlsxPath))
                {
                    try { File.Delete(tempXlsxPath); } catch { }
                }

                throw;
            }
        }


        #endregion
    }
}