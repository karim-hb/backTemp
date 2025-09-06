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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Receipts");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Recipt
                        .ProjectTo<ReciptResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Searchs = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Searchs.Data, _Meta: Searchs.Meta, _Header: header);

        }
        #endregion
        #region ActiveReserve
        // ------------------
        //  ActiveReserve
        // ------------------
        public async Task<ApiResponse> ActiveReserve(string customerIds, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(customerIds))
                return new ApiErrorResponse(StatusCodes.Status400BadRequest, "شناسه مشتری ارسال نشده است");

            var idStrings = customerIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            var parseErrors = new List<string>();
            var customerIdList = new List<int>();
            foreach (var s in idStrings)
            {
                if (int.TryParse(s, out var id))
                    customerIdList.Add(id);
                else
                    parseErrors.Add($"شناسه نامعتبر: '{s}'");
            }

            if (customerIdList.Count == 0)
            {
                var err = parseErrors.Count > 0 ? string.Join("; ", parseErrors) : "هیچ شناسه معتبری ارسال نشده است";
                return new ApiErrorResponse(StatusCodes.Status400BadRequest, err);
            }

            var customers = await _NarijeDBContext.Customers
                .Where(c => customerIdList.Contains(c.Id))
                .ToListAsync();

            var errors = new List<string>();

            var foundIds = customers.Select(c => c.Id).ToHashSet();
            var missingIds = customerIdList.Except(foundIds).ToList();
            if (missingIds.Any())
            {
                errors.AddRange(missingIds.Select(id => $"شرکت با شناسه {id} یافت نشد"));
            }

            // check each customer for Active and reserves
            foreach (var customer in customers)
            {
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

                if (!customer.Active)
                {
                    errors.Add($"{customerFullTitle}: این شعبه غیرفعال است");
                    continue;
                }

                var exists = await _NarijeDBContext.vReserves
                    .AnyAsync(r => r.CustomerId == customer.Id
                                   && r.DateTime.Date == date.Date
                                   && r.Num > 0
                                   && r.State != (int)EnumReserveState.perdict);

                if (!exists)
                {
                    errors.Add($"{customerFullTitle}: این شرکت هیچ رزروی ندارد");
                }
            }

            if (parseErrors.Any())
            {
                errors.InsertRange(0, parseErrors);
            }

            if (errors.Any())
            {
                var message = string.Join(" | ", errors);
                return new ApiErrorResponse(StatusCodes.Status400BadRequest, message);
            }

            return new ApiOkResponse(_Message: "SUCCESS");
        }
        #endregion




        #region ExportRecipt

        public async Task<FileContentResult> ExportRecipt(string customerIds, DateTime date, bool all = false)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();

            string xlsxPath = null;
            Recipt recipt = null;

            try
            {
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/ReciptTemplate.xlsx");
                using var package = new ExcelPackage(new FileInfo(templatePath));
                var wsTemplate = package.Workbook.Worksheets[0];
                var wsResult = package.Workbook.Worksheets.Add("Result");

                // Match column widths and sheet view with the template on the result sheet
                for (int col = 1; col <= 7; col++)
                {
                    wsResult.Column(col).Width = wsTemplate.Column(col).Width;
                }
                wsResult.View.RightToLeft = wsTemplate.View.RightToLeft;

                // --- Parse customerIds string ---
                List<int> customerIdList = new List<int>();
                if (!string.IsNullOrWhiteSpace(customerIds) && !all)
                {
                    foreach (var idStr in customerIds.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var trimmed = idStr.Trim();
                        if (int.TryParse(trimmed, out var id))
                            customerIdList.Add(id);
                    }
                }

                // --- Determine customers to include ---
                List<Customer> customers;
                if (all)
                {
                    var customerIdsForDay = await _NarijeDBContext.vReserves
                        .Where(r => r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict)
                        .Select(r => r.CustomerId)
                        .Distinct()
                        .ToListAsync();

                    customers = await _NarijeDBContext.Customers
                        .Where(c => customerIdsForDay.Contains(c.Id))
                        .OrderBy(c => c.Title)
                        .ToListAsync();
                }
                else
                {
                    var customersQuery = await _NarijeDBContext.Customers
                        .Where(c => customerIdList.Contains(c.Id))
                        .ToListAsync();
                    // Preserve the order provided by the user
                    var map = customersQuery.ToDictionary(c => c.Id, c => c);
                    customers = customerIdList.Where(id => map.ContainsKey(id)).Select(id => map[id]).ToList();
                }

                var persianCalendar = new PersianCalendar();
                string shamsiDate = string.Format("{0:0000}/{1:00}/{2:00}",
                    persianCalendar.GetYear(date),
                    persianCalendar.GetMonth(date),
                    persianCalendar.GetDayOfMonth(date));

                int currentRow = 1; // start writing rows (1-based)
                var generatedSheetNames = new List<string>();

                foreach (var customer in customers)
                {
                    // Clone template within the same package to preserve styles
                    var wsCustomer = package.Workbook.Worksheets.Add($"C_{customer.Id}", wsTemplate);

                    // Fetch parent title if exists
                    string parentTitle = null;
                    if (customer?.ParentId.HasValue == true)
                    {
                        parentTitle = await _NarijeDBContext.Customers
                            .Where(p => p.Id == customer.ParentId.Value)
                            .Select(p => p.Title)
                            .FirstOrDefaultAsync();
                    }

                    var customerFullTitle = (string.IsNullOrWhiteSpace(parentTitle)
                        ? customer.Title
                        : $"{customer.Title} - {parentTitle}");

                    // Fill header cells (single-customer logic)
                    wsCustomer.Cells["B2"].Value = customerFullTitle;
                    wsCustomer.Cells["D2"].Value = customer?.DeliverFullName ?? string.Empty;
                    wsCustomer.Cells["G2"].Value = customer?.Address ?? string.Empty;
                    wsCustomer.Cells["B3"].Value = customer?.Code ?? string.Empty;
                    wsCustomer.Cells["D3"].Value = customer?.DeliverPhoneNumber ?? string.Empty;
                    wsCustomer.Cells["G3"].Value = shamsiDate;

                    // Query reserves for the given date and this customer
                    var reserveRecords = await _NarijeDBContext.vReserves
                        .Where(r => r.DateTime.Date == date.Date && r.CustomerId == customer.Id && r.Num > 0 && r.State != (int)EnumReserveState.perdict)
                        .OrderBy(r => r.FoodTitle)
                        .ToListAsync();

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
                        wsCustomer.InsertRow(nextSectionRow, extraRows);

                        int styleA = wsCustomer.Cells[11, 1].StyleID;
                        int styleB = wsCustomer.Cells[11, 2].StyleID;
                        int styleC = wsCustomer.Cells[11, 3].StyleID;
                        int styleD = wsCustomer.Cells[11, 4].StyleID;
                        int styleE = wsCustomer.Cells[11, 5].StyleID;
                        int styleF = wsCustomer.Cells[11, 6].StyleID;
                        int styleG = wsCustomer.Cells[11, 7].StyleID;
                        double rowHeight = wsCustomer.Row(11).Height;

                        for (int r = nextSectionRow; r < nextSectionRow + extraRows; r++)
                        {
                            wsCustomer.Row(r).Height = rowHeight;
                            wsCustomer.Cells[r, 1].StyleID = styleA;
                            wsCustomer.Cells[r, 2].StyleID = styleB;
                            wsCustomer.Cells[r, 3].StyleID = styleC;
                            wsCustomer.Cells[r, 4].StyleID = styleD;
                            wsCustomer.Cells[r, 5].StyleID = styleE;
                            wsCustomer.Cells[r, 6].StyleID = styleF;
                            wsCustomer.Cells[r, 7].StyleID = styleG;

                            wsCustomer.Cells[r, 3, r, 4].Merge = true;
                            wsCustomer.Cells[r, 6, r, 7].Merge = true;
                        }
                    }

                    int row = startRow;
                    foreach (var item in reserves)
                    {
                        wsCustomer.Cells[row, 1].Value = row - startRow + 1;      // index
                        wsCustomer.Cells[row, 2].Value = item.FoodCode;
                        wsCustomer.Cells[row, 3].Value = item.FoodTitle;
                        wsCustomer.Cells[row, 5].Value = item.Quantity;
                        wsCustomer.Cells[row, 6].Value = string.Empty;
                        wsCustomer.Cells[row, 6].Style.Font.Bold = true;
                        wsCustomer.Cells[row, 7].Style.Font.Bold = true; // cover merged F:G
                        row++;
                    }

                    // Document code into header
                    wsCustomer.Cells["G1"].Value = $"کد سند: SP-F-ST-010-00";

                    // Append this customer's sheet to the result as a block (styles, merges preserved)
                    if (wsCustomer.Dimension != null)
                    {
                        int maxRow = wsCustomer.Dimension.End.Row;
                        var source = wsCustomer.Cells[1, 1, maxRow, 7]; // copy only A..G
                        var destination = wsResult.Cells[currentRow, 1];
                        source.Copy(destination);

                        // Copy row heights explicitly to match template
                        int rows = maxRow;
                        for (int i = 0; i < rows; i++)
                        {
                            double h = wsCustomer.Row(i + 1).Height;
                            if (h > 0)
                                wsResult.Row(currentRow + i).Height = h;
                        }

                        // Copy merged regions from the source sheet into the destination at the correct offset
                        int rowOffset = currentRow - 1;
                        int colOffset = 0;
                        foreach (var mergedAddress in wsCustomer.MergedCells)
                        {
                            if (string.IsNullOrWhiteSpace(mergedAddress))
                                continue;
                            var addr = new OfficeOpenXml.ExcelAddressBase(mergedAddress);
                            if (addr.Start.Column > 7) continue; // skip merges beyond G
                            int nsr = addr.Start.Row + rowOffset;
                            int nsc = addr.Start.Column + colOffset;
                            int ner = addr.End.Row + rowOffset;
                            int nec = Math.Min(addr.End.Column + colOffset, 7);
                            wsResult.Cells[nsr, nsc, ner, nec].Merge = true;
                        }

                        currentRow += rows + 1; // blank separator row
                    }

                    generatedSheetNames.Add(wsCustomer.Name);
                }

                // --- Create Recipt record for this combined file ---
                var identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((identity is null) || (identity.Claims.Count() == 0))
                    throw new Exception("دسترسی ندارید");
                var userId = Int32.Parse(identity.FindFirst("Id").Value);

                var allReserveIds = await _NarijeDBContext.vReserves
                    .Where(r => r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict
                        && (all || customerIdList.Contains(r.CustomerId)))
                    .Select(r => r.Id)
                    .Distinct()
                    .ToListAsync();

                recipt = new Recipt
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CustomerId = null,
                    CustomerParentId = null,
                    ReserveIds = string.Join(",", allReserveIds),
                    FileType = (int)EnumFileType.xlsx,
                    FileName = string.Empty
                };

                await _NarijeDBContext.Recipt.AddAsync(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                recipt.FileName = $"SP_{recipt.Id}";
                _NarijeDBContext.Recipt.Update(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                // Remove intermediate sheets and the template, keep only Result
                foreach (var name in generatedSheetNames)
                {
                    var s = package.Workbook.Worksheets[name];
                    if (s != null) package.Workbook.Worksheets.Delete(s);
                }
                package.Workbook.Worksheets.Delete(wsTemplate);

                var excelBytes = package.GetAsByteArray();

                // Save file
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

                var persianCalendar = new PersianCalendar();
                string shamsiDate = string.Format("{0:0000}/{1:00}/{2:00}",
                    persianCalendar.GetYear(date),
                    persianCalendar.GetMonth(date),
                    persianCalendar.GetDayOfMonth(date));


                ws.Cells["B2"].Value = customerFullTitle;
                ws.Cells["D2"].Value = customer?.DeliverFullName ?? string.Empty;
                ws.Cells["G2"].Value = customer?.Address ?? string.Empty;
                ws.Cells["B3"].Value = customer?.Code ?? string.Empty;
                ws.Cells["D3"].Value = customer?.DeliverPhoneNumber ?? string.Empty;
                ws.Cells["G3"].Value = shamsiDate;

                var reserveQuery = _NarijeDBContext.vReserves
                    .Where(r => r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict);
                if (customerId.HasValue)
                    reserveQuery = reserveQuery.Where(r => r.CustomerId == customerId.Value);

                var reserveRecords = await reserveQuery.OrderBy(r => r.FoodTitle).ToListAsync();
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
                    ws.Cells[row, 6].Style.Font.Bold = true;
                    ws.Cells[row, 7].Style.Font.Bold = true;
                    row++;
                }

                var identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((identity is null) || (identity.Claims.Count() == 0))
                    throw new Exception("دسترسی ندارید");
                var userId = Int32.Parse(identity.FindFirst("Id").Value);

                int? customerParentId = customer?.ParentId;

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

                ws.Cells["G1"].Value = $"کد سند: SP-F-ST-010-00";

                var basePath = "/data/recipts";
                Directory.CreateDirectory(basePath);
                tempXlsxPath = Path.Combine(basePath, $"{recipt.FileName}.xlsx");
                await File.WriteAllBytesAsync(tempXlsxPath, package.GetAsByteArray());

                var libreOfficePathCandidates = new[]
                {
            "/usr/bin/soffice",
            "/usr/lib/libreoffice/program/soffice",
            @"C:\Program Files\LibreOffice\program\soffice.exe",
            @"C:\Program Files (x86)\LibreOffice\program\soffice.exe"
        };
                var sofficePath = libreOfficePathCandidates.FirstOrDefault(File.Exists);
                if (string.IsNullOrEmpty(sofficePath))
                    throw new Exception("LibreOffice (soffice) is not installed in the runtime environment.");

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = sofficePath;
                process.StartInfo.Arguments = $"--headless --nologo --convert-to pdf:calc_pdf_Export --outdir \"{basePath}\" \"{tempXlsxPath}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                string stdOut = await process.StandardOutput.ReadToEndAsync();
                string stdErr = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode != 0)
                    throw new Exception($"LibreOffice conversion failed. ExitCode={process.ExitCode}. Error={stdErr}. Output={stdOut}");

                pdfPath = Path.Combine(basePath, $"{recipt.FileName}.pdf");
                if (!File.Exists(pdfPath))
                {
                    var altPdf = Path.Combine(basePath, $"{recipt.FileName}.PDF");
                    if (File.Exists(altPdf)) pdfPath = altPdf;
                    else throw new Exception($"Expected PDF not found after conversion. StdErr={stdErr}");
                }

                try { if (File.Exists(tempXlsxPath)) File.Delete(tempXlsxPath); } catch { }

                await transaction.CommitAsync();

                var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
                return new FileContentResult(pdfBytes, "application/pdf")
                {
                    FileDownloadName = $"{recipt.FileName}.pdf"
                };
            }
            catch(Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }

                if (!string.IsNullOrWhiteSpace(pdfPath) && File.Exists(pdfPath)) { try { File.Delete(pdfPath); } catch { } }
                if (!string.IsNullOrWhiteSpace(tempXlsxPath) && File.Exists(tempXlsxPath)) { try { File.Delete(tempXlsxPath); } catch { } }

                throw;
            }
        }


        #endregion
    }
}