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
        public async Task<ApiResponse> ActiveReserve(string customerIds, DateTime date, bool all = false)
        {
            List<int> customerIdList = new List<int>();
            
            if (!all)
            {
                if (string.IsNullOrWhiteSpace(customerIds))
                    return new ApiErrorResponse(StatusCodes.Status400BadRequest, "شناسه مشتری ارسال نشده است");

                var idStrings = customerIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();

                var parseErrors = new List<string>();
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
            }
            else
            {
                // Get all customers that have reserves for the date
                customerIdList = await _NarijeDBContext.vReserves
                    .Where(r => r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict)
                    .Select(r => r.CustomerId)
                    .Distinct()
                    .ToListAsync();
                    
                if (customerIdList.Count == 0)
                    return new ApiErrorResponse(StatusCodes.Status404NotFound, "هیچ رزروی برای این تاریخ یافت نشد");
            }

            var customers = await _NarijeDBContext.Customers
                .Where(c => customerIdList.Contains(c.Id))
                .ToListAsync();

            var errors = new List<string>();

            if (!all)
            {
                var foundIds = customers.Select(c => c.Id).ToHashSet();
                var missingIds = customerIdList.Except(foundIds).ToList();
                if (missingIds.Any())
                {
                    errors.AddRange(missingIds.Select(id => $"شرکت با شناسه {id} یافت نشد"));
                }
            }

            // Check each customer for Active and reserves
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

            if (errors.Any())
            {
                var message = string.Join(" | ", errors);
                return new ApiErrorResponse(StatusCodes.Status400BadRequest, message);
            }

            return new ApiOkResponse(_Message: "SUCCESS");
        }
        #endregion

        #region ExportAsync
        public async Task<ApiResponse> ExportAsync()
        {
            // Implementation if needed
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
                // Parse customer IDs
                List<int> customerIdList = new List<int>();
                if (!all && !string.IsNullOrWhiteSpace(customerIds))
                {
                    customerIdList = customerIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.Parse(s.Trim()))
                        .ToList();
                }

                // Get customers
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
                else if (customerIdList.Any())
                {
                    customers = await _NarijeDBContext.Customers
                        .Where(c => customerIdList.Contains(c.Id))
                        .OrderBy(c => c.Title)
                        .ToListAsync();
                }
                else
                {
                    // No customers specified
                    customers = new List<Customer>();
                }

                // Create new Excel package
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add("Receipts");
                
                // Track all reserve IDs for the receipt record
                var allReserveIds = new List<int>();
                
                // Persian date
                var persianCalendar = new PersianCalendar();
                string shamsiDate = string.Format("{0:0000}/{1:00}/{2:00}",
                    persianCalendar.GetYear(date),
                    persianCalendar.GetMonth(date),
                    persianCalendar.GetDayOfMonth(date));

                int currentRow = 1;
                
                // Process each customer
                for (int custIndex = 0; custIndex < customers.Count; custIndex++)
                {
                    var customer = customers[custIndex];
                    
                    // Add spacing between templates (except for the first one)
                    if (custIndex > 0)
                    {
                        currentRow++; // Add one empty row between templates
                    }
                    
                    // Get parent title if exists
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

                    // Create template structure for this customer
                    // Row 1: Header with document code
                    ws.Cells[currentRow, 1].Value = "ردیف";
                    ws.Cells[currentRow, 2].Value = "کد کالا";
                    ws.Cells[currentRow, 3].Value = "نام کالا";
                    ws.Cells[currentRow, 5].Value = "مقدار/تعداد";
                    ws.Cells[currentRow, 6].Value = "توضیحات";
                    ws.Cells[currentRow, 7].Value = $"کد سند: SP-F-ST-010-00";
                    
                    // Apply header styling
                    using (var range = ws.Cells[currentRow, 1, currentRow, 7])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }
                    currentRow++;

                    // Row 2: Customer info
                    ws.Cells[currentRow, 1].Value = "نام مشتری:";
                    ws.Cells[currentRow, 2].Value = customerFullTitle;
                    ws.Cells[currentRow, 2, currentRow, 3].Merge = true;
                    ws.Cells[currentRow, 4].Value = "تحویل گیرنده:";
                    ws.Cells[currentRow, 5].Value = customer.DeliverFullName ?? string.Empty;
                    ws.Cells[currentRow, 6].Value = "آدرس:";
                    ws.Cells[currentRow, 7].Value = customer.Address ?? string.Empty;
                    currentRow++;

                    // Row 3: Additional info
                    ws.Cells[currentRow, 1].Value = "کد مشتری:";
                    ws.Cells[currentRow, 2].Value = customer.Code ?? string.Empty;
                    ws.Cells[currentRow, 2, currentRow, 3].Merge = true;
                    ws.Cells[currentRow, 4].Value = "شماره تماس:";
                    ws.Cells[currentRow, 5].Value = customer.DeliverPhoneNumber ?? string.Empty;
                    ws.Cells[currentRow, 6].Value = "تاریخ:";
                    ws.Cells[currentRow, 7].Value = shamsiDate;
                    currentRow++;

                    // Empty row before data headers
                    currentRow++;

                    // Data headers row
                    ws.Cells[currentRow, 1].Value = "ردیف";
                    ws.Cells[currentRow, 2].Value = "کد کالا";
                    ws.Cells[currentRow, 3].Value = "نام کالا";
                    ws.Cells[currentRow, 3, currentRow, 4].Merge = true;
                    ws.Cells[currentRow, 5].Value = "مقدار/تعداد";
                    ws.Cells[currentRow, 6].Value = "توضیحات";
                    ws.Cells[currentRow, 6, currentRow, 7].Merge = true;
                    
                    // Style data headers
                    using (var range = ws.Cells[currentRow, 1, currentRow, 7])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    currentRow++;

                    // Query reserves for this customer
                    var reserveRecords = await _NarijeDBContext.vReserves
                        .Where(r => r.DateTime.Date == date.Date 
                                 && r.CustomerId == customer.Id 
                                 && r.Num > 0 
                                 && r.State != (int)EnumReserveState.perdict)
                        .OrderBy(r => r.FoodTitle)
                        .ToListAsync();

                    var reserveIds = reserveRecords.Select(r => r.Id).Distinct().ToList();
                    allReserveIds.AddRange(reserveIds);

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

                    // Fill data rows
                    int dataStartRow = currentRow;
                    int itemNumber = 1;
                    foreach (var item in reserves)
                    {
                        ws.Cells[currentRow, 1].Value = itemNumber++;
                        ws.Cells[currentRow, 2].Value = item.FoodCode;
                        ws.Cells[currentRow, 3].Value = item.FoodTitle;
                        ws.Cells[currentRow, 3, currentRow, 4].Merge = true;
                        ws.Cells[currentRow, 5].Value = item.Quantity;
                        ws.Cells[currentRow, 6].Value = string.Empty;
                        ws.Cells[currentRow, 6, currentRow, 7].Merge = true;
                        
                        // Apply borders to data rows
                        using (var range = ws.Cells[currentRow, 1, currentRow, 7])
                        {
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }
                        currentRow++;
                    }

                    // Add minimum 6 rows even if less data (to match template structure)
                    int minRows = 6;
                    int rowsAdded = reserves.Count;
                    while (rowsAdded < minRows)
                    {
                        ws.Cells[currentRow, 1].Value = itemNumber++;
                        ws.Cells[currentRow, 2].Value = string.Empty;
                        ws.Cells[currentRow, 3].Value = string.Empty;
                        ws.Cells[currentRow, 3, currentRow, 4].Merge = true;
                        ws.Cells[currentRow, 5].Value = string.Empty;
                        ws.Cells[currentRow, 6].Value = string.Empty;
                        ws.Cells[currentRow, 6, currentRow, 7].Merge = true;
                        
                        // Apply borders to empty rows
                        using (var range = ws.Cells[currentRow, 1, currentRow, 7])
                        {
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }
                        currentRow++;
                        rowsAdded++;
                    }

                    // Add signature section
                    currentRow++; // Empty row
                    ws.Cells[currentRow, 2].Value = "امضاء تحویل دهنده:";
                    ws.Cells[currentRow, 5].Value = "امضاء تحویل گیرنده:";
                    currentRow++;
                    currentRow++; // Extra spacing after each template
                }

                // Auto-fit columns
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                
                // Set minimum column widths
                ws.Column(1).Width = 8;  // ردیف
                ws.Column(2).Width = 12; // کد کالا
                ws.Column(3).Width = 25; // نام کالا
                ws.Column(4).Width = 15; // (merged with نام کالا)
                ws.Column(5).Width = 15; // مقدار/تعداد
                ws.Column(6).Width = 20; // توضیحات
                ws.Column(7).Width = 20; // (merged with توضیحات)

                // Create Recipt record
                var identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((identity is null) || (identity.Claims.Count() == 0))
                    throw new Exception("دسترسی ندارید");
                var userId = Int32.Parse(identity.FindFirst("Id").Value);

                recipt = new Recipt
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CustomerId = customers.Count == 1 ? (int?)customers[0].Id : null,
                    CustomerParentId = customers.Count == 1 ? customers[0].ParentId : null,
                    ReserveIds = string.Join(",", allReserveIds.Distinct()),
                    FileType = (int)EnumFileType.xlsx,
                    FileName = string.Empty
                };

                await _NarijeDBContext.Recipt.AddAsync(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                recipt.FileName = $"SP_{recipt.Id}";
                _NarijeDBContext.Recipt.Update(recipt);
                await _NarijeDBContext.SaveChangesAsync();

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
        public async Task<FileContentResult> ExportPdfRecipt(string customerIds, DateTime date, bool all = false)
        {
            using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();

            string pdfPath = null;
            string tempXlsxPath = null;
            Recipt recipt = null;

            try
            {
                // First generate the Excel file using the multi-customer logic
                var excelResult = await ExportRecipt(customerIds, date, all);
                var excelBytes = excelResult.FileContents;

                // Create Recipt record for PDF
                var identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((identity is null) || (identity.Claims.Count() == 0))
                    throw new Exception("دسترسی ندارید");
                var userId = Int32.Parse(identity.FindFirst("Id").Value);

                // Parse customer IDs
                List<int> customerIdList = new List<int>();
                if (!all && !string.IsNullOrWhiteSpace(customerIds))
                {
                    customerIdList = customerIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.Parse(s.Trim()))
                        .ToList();
                }

                // Get all reserve IDs
                var allReserveIds = new List<int>();
                if (all)
                {
                    allReserveIds = await _NarijeDBContext.vReserves
                        .Where(r => r.DateTime.Date == date.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict)
                        .Select(r => r.Id)
                        .Distinct()
                        .ToListAsync();
                }
                else if (customerIdList.Any())
                {
                    allReserveIds = await _NarijeDBContext.vReserves
                        .Where(r => r.DateTime.Date == date.Date 
                                 && customerIdList.Contains(r.CustomerId) 
                                 && r.Num > 0 
                                 && r.State != (int)EnumReserveState.perdict)
                        .Select(r => r.Id)
                        .Distinct()
                        .ToListAsync();
                }

                recipt = new Recipt
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CustomerId = customerIdList.Count == 1 ? (int?)customerIdList[0] : null,
                    CustomerParentId = null,
                    ReserveIds = string.Join(",", allReserveIds),
                    FileType = (int)EnumFileType.pdf,
                    FileName = string.Empty
                };

                await _NarijeDBContext.Recipt.AddAsync(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                recipt.FileName = $"SP_{recipt.Id}";
                _NarijeDBContext.Recipt.Update(recipt);
                await _NarijeDBContext.SaveChangesAsync();

                // Save temporary Excel file
                var basePath = "/data/recipts";
                Directory.CreateDirectory(basePath);
                tempXlsxPath = Path.Combine(basePath, $"{recipt.FileName}_temp.xlsx");
                await File.WriteAllBytesAsync(tempXlsxPath, excelBytes);

                // Convert to PDF using LibreOffice
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

                pdfPath = Path.Combine(basePath, $"{recipt.FileName}_temp.pdf");
                if (!File.Exists(pdfPath))
                {
                    var altPdf = Path.Combine(basePath, $"{recipt.FileName}_temp.PDF");
                    if (File.Exists(altPdf)) pdfPath = altPdf;
                    else throw new Exception($"Expected PDF not found after conversion. StdErr={stdErr}");
                }

                // Rename to final name
                var finalPdfPath = Path.Combine(basePath, $"{recipt.FileName}.pdf");
                if (File.Exists(finalPdfPath))
                    File.Delete(finalPdfPath);
                File.Move(pdfPath, finalPdfPath);
                pdfPath = finalPdfPath;

                // Clean up temp Excel file
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