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
using Narije.Core.DTOs.ViewModels.Reserve;
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
    public class ReserveRepository : BaseRepository<Reserve>, IReserveRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public ReserveRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Reserve = await _NarijeDBContext.vReserves
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<ReserveResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Reserve);
        }
        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit, bool byUser = false, bool justPredict = false)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Reserve");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.vReserves.Where(c => c.Num != 0);

            if (justPredict)
            {
                Q = Q.Where(c => c.State == (int)EnumReserveState.perdict);
            }
            else
            {
                Q = Q.Where(c => c.State != (int)EnumReserveState.perdict);
            }

            Q = Q.OrderByDescending(c => c.Id);

            var projectedQ = Q.ProjectTo<ReserveResponse>(_IMapper.ConfigurationProvider);

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

            var user = await _NarijeDBContext.Users
                .Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (user is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

            switch (user.Role)
            {
                case (int)EnumRole.user:
                    projectedQ = projectedQ.Where(A => A.userId == user.Id);
                    break;
                case (int)EnumRole.customer:
                    projectedQ = projectedQ.Where(A => A.customerId == user.CustomerId);
                    break;
            }

            if ((query.Search != "") || (query.Filter.Count > 0) || (query.Sort.Count > 0))
            {
                projectedQ = projectedQ.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);
            }

            var Reserves = await projectedQ.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Reserves.Data, _Meta: Reserves.Meta, _Header: header);
        }
        #endregion

        #region ExportBranchServicesAsync
        public async Task<FileContentResult> ExportBranchServicesAsync(DateTime fromData, DateTime toData, bool predict)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Reserve");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.vReserves.Where(c => c.Num != 0);

            if (predict == true)
            {
                Q = Q.Where(r => r.DateTime.Date >= fromData.Date && r.DateTime.Date <= toData.Date && r.State == (int)EnumReserveState.perdict);

            }
            else
            {
                Q = Q.Where(r => r.DateTime.Date >= fromData.Date && r.DateTime.Date <= toData.Date && r.State != (int)EnumReserveState.perdict);


            }

            var reserves = await Q.ToListAsync();
            var allMealTitles = _NarijeDBContext.Meal.Select(m => m.Title).ToList();

            if (!reserves.Any())
                throw new Exception("  در این بازه رزرو یافت نشد");

            var groupedReserves = reserves
                .GroupBy(r => r.BranchTitle)
                .Select(g => new
                {
                    BranchTitle = g.Key,
                    PeriodStart = Q.Min(r => r.DateTime),
                    PeriodEnd = Q.Max(r => r.DateTime),
                    FoodTotals = g.GroupBy(r => r.FoodTitle)
                                  .ToDictionary(fg => fg.Key, fg => fg.Sum(r => r.Num)),
                    MealTotals = g.GroupBy(r => r.MealTitle)
                                  .ToDictionary(mg => mg.Key, mg => mg.Sum(r => r.Num)),
                    TotalSum = g.Sum(r => r.Num),
                    TotalPriceSum = g.Sum(r => r.Num * r.Price * 10),
                    Companies = g.GroupBy(r => r.CustomerTitle)
                         .Select(cg => new
                         {
                             CompanyTitle = cg.Key,
                             FoodTotals = cg.GroupBy(r => r.FoodTitle)
                                            .ToDictionary(fg => fg.Key, fg => fg.Sum(r => r.Num)),
                             MealTotals = cg.GroupBy(r => r.MealTitle)
                                  .ToDictionary(mg => mg.Key, mg => mg.Sum(r => r.Num)),
                             TotalSum = cg.Sum(r => r.Num),
                             TotalPriceSum = cg.Sum(r => r.Num * r.Price * 10),
                         })
                         .ToList()
                })
                .ToList();

            var allFoodTitle = reserves
                .Select(item => item.FoodTitle)
                .Distinct()
                .ToList();

            var persianCalendar = new PersianCalendar();
            var periodStart = groupedReserves.Min(g => g.PeriodStart);
            var periodEnd = groupedReserves.Max(g => g.PeriodEnd);
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromData)}/{persianCalendar.GetMonth(fromData):D2}/{persianCalendar.GetDayOfMonth(fromData):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toData)}/{persianCalendar.GetMonth(toData):D2}/{persianCalendar.GetDayOfMonth(toData):D2}";
            var shamsiDate = $"{persianCalendar.GetYear(DateTime.Now)}/{persianCalendar.GetMonth(DateTime.Now):D2}/{persianCalendar.GetDayOfMonth(DateTime.Now):D2}";
            var fileName = $"خروجی خدمات شعبه ها {shamsiDate}.xlsx";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Branch Services");

                // Title
                worksheet.Cells[1, 2].Value = $"چارتر سفارشات فروش در بازه :‌ {shamsiPeriodStart} - {shamsiPeriodEnd}";
                worksheet.Cells[1, 2, 1, allFoodTitle.Count + allMealTitles.Count + 4].Merge = true;
                worksheet.Cells[1, 2].Style.Font.Size = 16;
                worksheet.Cells[1, 2].Style.Font.Bold = true;
                worksheet.Cells[1, 2].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells[1, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(Color.Black);
                worksheet.Row(1).Height = 25;

                // Header Row
                worksheet.Cells[2, 2].Value = "نام شعبه";

                int colIndex = 3;
                foreach (var foodTitle in allFoodTitle)
                {
                    worksheet.Cells[2, colIndex].Value = foodTitle;
                    colIndex++;
                }

                foreach (var mealTitle in allMealTitles)
                {
                    worksheet.Cells[2, colIndex].Value = mealTitle;
                    colIndex++;
                }

                worksheet.Cells[2, colIndex].Value = "جمع تعداد";
                colIndex++;
                worksheet.Cells[2, colIndex].Value = "جمع قیمت";

                // Sum Row
                worksheet.Cells[3, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[3, 2].Style.Fill.BackgroundColor.SetColor(Color.White);

                colIndex = 3;
                worksheet.Cells[3, 2].Value = "جمع همه شعبه ها";
                foreach (var foodTitle in allFoodTitle)
                {
                    worksheet.Cells[3, colIndex].Value = groupedReserves.Sum(branch => branch.FoodTotals.ContainsKey(foodTitle) ? branch.FoodTotals[foodTitle] : 0);
                    colIndex++;
                }

                foreach (var mealTitle in allMealTitles)
                {
                    worksheet.Cells[3, colIndex].Value = groupedReserves.Sum(branch => branch.MealTotals.ContainsKey(mealTitle) ? branch.MealTotals[mealTitle] : 0);
                    colIndex++;
                }

                worksheet.Cells[3, colIndex].Value = groupedReserves.Sum(branch => branch.TotalSum);
                colIndex++;
                worksheet.Cells[3, colIndex].Value = groupedReserves.Sum(branch => branch.TotalPriceSum);

                // Data Rows with Alternating Colors
                int rowIndex = 4;
                var colors = new[] { Color.LightGreen, Color.Yellow, Color.LightBlue };
                var branchColors = new Dictionary<string, Color>();
                foreach (var branch in groupedReserves)
                {
                    var backgroundColor = colors[(rowIndex - 4) % colors.Length];
                    branchColors[branch.BranchTitle] = backgroundColor;
                    worksheet.Cells[rowIndex, 2].Value = branch.BranchTitle;

                    colIndex = 3;
                    foreach (var foodTitle in allFoodTitle)
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = branch.FoodTotals.ContainsKey(foodTitle) ? branch.FoodTotals[foodTitle] : 0;
                        colIndex++;
                    }

                    foreach (var mealTitle in allMealTitles)
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = branch.MealTotals.ContainsKey(mealTitle) ? branch.MealTotals[mealTitle] : 0;
                        colIndex++;
                    }

                    worksheet.Cells[rowIndex, colIndex].Value = branch.TotalSum;
                    colIndex++;
                    worksheet.Cells[rowIndex, colIndex].Value = branch.TotalPriceSum;

                    // Set background color
                    worksheet.Cells[rowIndex, 2, rowIndex, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[rowIndex, 2, rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(backgroundColor);

                    rowIndex++;


                }

                worksheet.Cells[rowIndex, 2, rowIndex, colIndex].Merge = true;
                worksheet.Cells[rowIndex, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[rowIndex, 2].Style.Fill.BackgroundColor.SetColor(Color.Red);
                worksheet.Row(rowIndex).Height = 4;
                rowIndex += 1;

                foreach (var branch in groupedReserves)

                {
                    var backgroundColor = branchColors.ContainsKey(branch.BranchTitle) ? branchColors[branch.BranchTitle] : Color.White;
                    worksheet.Cells[rowIndex, 1].Value = branch.BranchTitle;
                    foreach (var company in branch.Companies)
                    {
                        worksheet.Cells[rowIndex, 2].Value = company.CompanyTitle;

                        colIndex = 3;
                        foreach (var foodTitle in allFoodTitle)
                        {
                            worksheet.Cells[rowIndex, colIndex].Value = company.FoodTotals.ContainsKey(foodTitle) ? company.FoodTotals[foodTitle] : 0;
                            colIndex++;
                        }

                        foreach (var mealTitle in allMealTitles)
                        {
                            worksheet.Cells[rowIndex, colIndex].Value = company.MealTotals.ContainsKey(mealTitle) ? company.MealTotals[mealTitle] : 0;
                            colIndex++;
                        }

                        worksheet.Cells[rowIndex, colIndex].Value = company.TotalSum;
                        colIndex++;
                        worksheet.Cells[rowIndex, colIndex].Value = company.TotalPriceSum;

                        // Apply branch background color to company rows
                        worksheet.Cells[rowIndex, 1, rowIndex, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheet.Cells[rowIndex, 1, rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(backgroundColor);

                        rowIndex++;
                    }
                }
                // Styling
                worksheet.Cells[1, 1, rowIndex, colIndex].Style.Font.Name = "Tahoma";
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var excelBytes = package.GetAsByteArray();

                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }


        #endregion

        #region ExportFoodBaseOnDayAsync
        public async Task<FileContentResult> ExportFoodBaseOnDayAsync(DateTime fromDate, DateTime toDate, bool isFood)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var reserves = await _NarijeDBContext.vReserves
                .Where(r => r.DateTime.Date >= fromDate.Date && r.DateTime.Date <= toDate.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict && r.IsFood == true)
                .ToListAsync();

            if (!reserves.Any())
                throw new Exception("در این بازه رزرو یافت نشد");

            var meals = await _NarijeDBContext.Meal.ToListAsync();

            var persianCalendar = new PersianCalendar();
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toDate)}/{persianCalendar.GetMonth(toDate):D2}/{persianCalendar.GetDayOfMonth(toDate):D2}";
            var shamsiDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
             .Select(offset => fromDate.AddDays(offset))
             .Select(date => (dynamic)new
             {
                 GregorianDate = date,
                 ShamsiDate = $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):D2}/{persianCalendar.GetDayOfMonth(date):D2}"
             })
                 .ToList();

            var fileName = $"گزارش تعداد غذا بر اساس تاریخ {DateTime.Now:yyyy-MM-dd}.xlsx";

            using (var package = new ExcelPackage())
            {
                foreach (var meal in meals)
                {
                    var mealReserves = reserves.Where(r => r.MealType == meal.Id).ToList();
                    CreateSheetForMeal(package, meal.Title, mealReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd);
                }

                CreateSheetForMeal(package, "همه", reserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd);

                var excelBytes = package.GetAsByteArray();

                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        private void CreateSheetForMeal(ExcelPackage package, string mealTitle, List<vReserve> reserves, List<dynamic> shamsiDates, string shamsiPeriodStart, string shamsiPeriodEnd)
        {
            var worksheet = package.Workbook.Worksheets.Add(mealTitle);

            worksheet.Cells[1, 1].Value = $"گزارش تجمیعی {mealTitle} , {shamsiPeriodStart} - {shamsiPeriodEnd}";
            worksheet.Cells[1, 1, 1, shamsiDates.Count + 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Font.Size = 18;
            worksheet.Cells[1, 1, 1, shamsiDates.Count + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);
            worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;


            worksheet.Cells[2, 1].Value = "کد کالا";
            worksheet.Cells[2, 2].Value = "دسته‌بندی";
            worksheet.Cells[2, 3].Value = "نام غذا";
            int colIndex = 4;
            foreach (var date in shamsiDates)
            {
                worksheet.Cells[2, colIndex].Value = date.ShamsiDate;
                colIndex++;
            }
            worksheet.Cells[2, colIndex].Value = "جمع ردیف";

            worksheet.Cells[3, 1].Value = "جمع ستون";
            worksheet.Cells[3, 1, 3, 3].Merge = true;

            colIndex = 4;
            var groupedData = reserves
                .GroupBy(r => r.DateTime.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(r => r.FoodTitle)
                        .ToDictionary(fg => fg.Key, fg => fg.Sum(r => r.Num))
                );

            foreach (var date in shamsiDates)
            {
                var dateKey = date.GregorianDate.Date;

                worksheet.Cells[3, colIndex].Value = reserves
                    .Where(r => r.DateTime.Date == dateKey)
                    .Sum(r => r.Num);
                colIndex++;
            }
            worksheet.Cells[3, colIndex].Value = reserves.Sum(r => r.Num);

            var allFoodTitles = reserves
                .Select(r => r.FoodTitle)
                .Distinct()
                .OrderBy(title => title)
                .ToList();

            int rowIndex = 4;
            foreach (var foodTitle in allFoodTitles)
            {
                var food = reserves.FirstOrDefault(r => r.FoodTitle == foodTitle);
                worksheet.Cells[rowIndex, 1].Value = food?.FoodArpaNumber ?? "";
                worksheet.Cells[rowIndex, 2].Value = food?.Category ?? "";

                worksheet.Cells[rowIndex, 3].Value = foodTitle;

                colIndex = 4;
                int rowSum = 0;
                foreach (var date in shamsiDates)
                {
                    var dateKey = date.GregorianDate.Date;
                    var value = groupedData.ContainsKey(dateKey) && groupedData[dateKey].ContainsKey(foodTitle)
                        ? groupedData[dateKey][foodTitle]
                        : 0;
                    worksheet.Cells[rowIndex, colIndex].Value = value;
                    rowSum += value;
                    colIndex++;
                }
                worksheet.Cells[rowIndex, colIndex].Value = rowSum;
                rowIndex++;
            }

            worksheet.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Font.Name = "Tahoma";
            worksheet.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].AutoFitColumns();
            worksheet.Cells[2, 1, 2, shamsiDates.Count + 4].Style.Font.Bold = true;
            worksheet.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            worksheet.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            worksheet.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            worksheet.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            worksheet.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
        }




        #endregion



        #region ExportDailyBaseOnBranchesAndFoodAsync
        public async Task<FileContentResult> ExportDailyBaseOnBranchesAndFoodAsync(DateTime fromDate, DateTime toDate, bool isFood)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var reserves = await _NarijeDBContext.vReserves
                .Where(r => r.DateTime.Date >= fromDate.Date && r.DateTime.Date <= toDate.Date && r.Num > 0 && r.State != (int)EnumReserveState.perdict && r.IsFood == true)
                .ToListAsync();

            if (!reserves.Any())
                throw new Exception("در این بازه رزرو یافت نشد");

            var persianCalendar = new PersianCalendar();
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toDate)}/{persianCalendar.GetMonth(toDate):D2}/{persianCalendar.GetDayOfMonth(toDate):D2}";

            var shamsiDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset))
                .Select(date => new
                {
                    GregorianDate = date,
                    ShamsiDate = $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):D2}/{persianCalendar.GetDayOfMonth(date):D2}"
                })
                .ToList();

            var meals = await _NarijeDBContext.Meal.ToListAsync();
            var colors = new[] { Color.LightGreen, Color.Yellow, Color.LightBlue };
            var fileName = $"گزارش تفکیکی بر اساس تاریخ {DateTime.Now:yyyy-MM-dd}.xlsx";
            using (var package = new ExcelPackage())
            {
                var worksheetAll = package.Workbook.Worksheets.Add("همه");
                worksheetAll.Cells[1, 1].Value = $"گزارش تفکیکی در بازه {shamsiPeriodStart} تا {shamsiPeriodEnd}";
                worksheetAll.Cells[1, 1, 1, shamsiDates.Count + 3].Merge = true;
                worksheetAll.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Font.Bold = true;
                worksheetAll.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Font.Size = 18;
                worksheetAll.Cells[1, 1, 1, shamsiDates.Count + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheetAll.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheetAll.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);


                worksheetAll.Cells[2, 1].Value = "کد کالا ";
                worksheetAll.Cells[2, 2].Value = " گروه کالا ";
                worksheetAll.Cells[2, 3].Value = "نام غذا";

                int colIndex = 4;
                foreach (var date in shamsiDates)
                {
                    worksheetAll.Cells[2, colIndex].Value = date.ShamsiDate;
                    colIndex++;
                }
                worksheetAll.Cells[2, colIndex].Value = "جمع ردیف";

                int rowIndex = 3;
                var colorInex1 = 0;
                foreach (var branchGroup in reserves.GroupBy(r => r.BranchTitle))
                {
                    worksheetAll.Cells[rowIndex, 1].Value = branchGroup.Key;
                    colIndex = 4;

                    var backgroundColor = colors[colorInex1];
                    colorInex1 = colorInex1 + 1;
                    foreach (var date in shamsiDates)
                    {
                        var dateKey = date.GregorianDate.Date;
                        var sum = branchGroup.Where(r => r.DateTime.Date == dateKey).Sum(r => r.Num);
                        worksheetAll.Cells[rowIndex, colIndex].Value = sum;
                        colIndex++;
                    }
                    worksheetAll.Cells[rowIndex, colIndex].Value = branchGroup.Sum(r => r.Num);
                    worksheetAll.Cells[rowIndex, 1, rowIndex, shamsiDates.Count + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheetAll.Cells[rowIndex, 1, rowIndex, shamsiDates.Count + 3].Style.Fill.BackgroundColor.SetColor(backgroundColor);
                    rowIndex++;

                    foreach (var foodGroup in branchGroup.GroupBy(r => r.FoodTitle))
                    {
                        worksheetAll.Cells[rowIndex, 1].Value = $"{foodGroup.First().FoodArpaNumber}";
                        worksheetAll.Cells[rowIndex, 2].Value = $"{foodGroup.First().Category}";
                        worksheetAll.Cells[rowIndex, 3].Value = foodGroup.Key;

                        colIndex = 4;
                        foreach (var date in shamsiDates)
                        {
                            var dateKey = date.GregorianDate.Date;
                            var foodSum = foodGroup.Where(r => r.DateTime.Date == dateKey).Sum(r => r.Num);
                            worksheetAll.Cells[rowIndex, colIndex].Value = foodSum;
                            colIndex++;
                        }
                        worksheetAll.Cells[rowIndex, colIndex].Value = foodGroup.Sum(r => r.Num);
                        rowIndex++;
                    }
                    worksheetAll.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 3].Style.Font.Name = "Tahoma";
                    worksheetAll.Cells[2, 1, 2, shamsiDates.Count + 3].Style.Font.Bold = true;
                    worksheetAll.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 3].AutoFitColumns();
                    worksheetAll.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    worksheetAll.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheetAll.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheetAll.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    worksheetAll.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }


                foreach (var meal in meals)
                {
                    var worksheetMeal = package.Workbook.Worksheets.Add(meal.Title);

                    worksheetMeal.Cells[1, 1].Value = $"{meal.Title} - گزارش تفکیکی در بازه {fromDate:yyyy-MM-dd} تا {toDate:yyyy-MM-dd}";
                    worksheetMeal.Cells[1, 1, 1, shamsiDates.Count + 3].Merge = true;
                    worksheetMeal.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Font.Bold = true;
                    worksheetMeal.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Font.Size = 18;
                    worksheetMeal.Cells[1, 1, 1, shamsiDates.Count + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheetMeal.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheetMeal.Cells[1, 1, 1, shamsiDates.Count + 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Orange);

                    worksheetMeal.Cells[2, 1].Value = "کد کالا ";
                    worksheetMeal.Cells[2, 2].Value = " گروه کالا ";
                    worksheetMeal.Cells[2, 3].Value = "نام غذا";

                    colIndex = 4;
                    foreach (var date in shamsiDates)
                    {
                        worksheetMeal.Cells[2, colIndex].Value = date.ShamsiDate;
                        colIndex++;
                    }
                    worksheetMeal.Cells[2, colIndex].Value = "جمع ردیف";

                    rowIndex = 3;
                    var foodGroupByMeal = reserves.Where(r => r.MealType == meal.Id).GroupBy(r => r.BranchTitle);

                    var colorInex = 0;
                    foreach (var branchGroup in foodGroupByMeal)
                    {


                        colorInex = colorInex + 1;
                        worksheetMeal.Cells[rowIndex, 1].Value = branchGroup.Key;
                        colIndex = 4;
                        foreach (var date in shamsiDates)
                        {
                            var dateKey = date.GregorianDate.Date;
                            var sum = branchGroup.Where(r => r.DateTime.Date == dateKey).Sum(r => r.Num);
                            worksheetMeal.Cells[rowIndex, colIndex].Value = sum;
                            colIndex++;
                        }
                        worksheetMeal.Cells[rowIndex, colIndex].Value = branchGroup.Sum(r => r.Num);
                        worksheetMeal.Cells[rowIndex, 1, rowIndex, shamsiDates.Count + 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        var backgroundColor = colors[colorInex];
                        worksheetMeal.Cells[rowIndex, 1, rowIndex, shamsiDates.Count + 3].Style.Fill.BackgroundColor.SetColor(backgroundColor);
                        rowIndex++;

                        foreach (var foodGroup in branchGroup.GroupBy(r => r.FoodTitle))
                        {
                            worksheetMeal.Cells[rowIndex, 1].Value = $"{foodGroup.First().FoodArpaNumber}";
                            worksheetMeal.Cells[rowIndex, 2].Value = $"{foodGroup.First().Category}";
                            worksheetMeal.Cells[rowIndex, 3].Value = foodGroup.Key;

                            colIndex = 4; // Starting from the food column
                            foreach (var date in shamsiDates)
                            {
                                var dateKey = date.GregorianDate.Date;
                                var foodSum = foodGroup.Where(r => r.DateTime.Date == dateKey).Sum(r => r.Num);
                                worksheetMeal.Cells[rowIndex, colIndex].Value = foodSum;
                                colIndex++;
                            }
                            worksheetMeal.Cells[rowIndex, colIndex].Value = foodGroup.Sum(r => r.Num);
                            rowIndex++;
                        }

                        worksheetMeal.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 3].Style.Font.Name = "Tahoma";
                        worksheetMeal.Cells[2, 1, 2, shamsiDates.Count + 3].Style.Font.Bold = true;
                        worksheetMeal.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 3].AutoFitColumns();
                        worksheetMeal.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 3].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                        worksheetMeal.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheetMeal.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheetMeal.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheetMeal.Cells[2, 1, rowIndex - 1, shamsiDates.Count + 4].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }
                }



                var excelBytes = package.GetAsByteArray();

                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }


        #endregion



        #region ExportReserveBaseOnTheFood
        public async Task<FileContentResult> ExportReserveBaseOnTheFood(DateTime fromDate, DateTime toDate, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var reservesQuery = _NarijeDBContext.vReserves
             .Where(r => r.DateTime.Date >= fromDate.Date && r.DateTime.Date <= toDate.Date && r.Num > 0);

            if (justPredict)
            {
                reservesQuery= reservesQuery.Where(c => c.State == (int)EnumReserveState.perdict);
            }
            else
            {
                reservesQuery= reservesQuery.Where(c => c.State != (int)EnumReserveState.perdict );
            }


            var company = await _NarijeDBContext.Settings
                                    .Select(A =>
                                     A.CompanyName)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

            List<int> foodGroupIdList = null;
            if (!string.IsNullOrEmpty(foodGroupIds))
            {
                foodGroupIdList = foodGroupIds.Split(',')
                                              .Select(id => int.Parse(id.Trim()))
                                              .ToList();
            }


            if (foodGroupIdList != null && foodGroupIdList.Any())
            {
                reservesQuery = reservesQuery.Where(r => foodGroupIdList.Contains(r.FoodGroupId ?? 0));
            }

            var reserves = await reservesQuery.ToListAsync();

            if (!reserves.Any())
                throw new Exception("در این بازه رزرو یافت نشد");

            List<AccessoryCompany> accessoryCompanies = null;
            if (showAccessory)
            {
                var customerIds = reserves.Select(r => r.CustomerId).Distinct().ToList();
                accessoryCompanies = await _NarijeDBContext.AccessoryCompany
                    .Include(ac => ac.Accessory)
                    .Where(ac => customerIds.Contains(ac.CompanyId))
                    .ToListAsync();
            }

            var persianCalendar = new PersianCalendar();
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toDate)}/{persianCalendar.GetMonth(toDate):D2}/{persianCalendar.GetDayOfMonth(toDate):D2}";

            var shamsiDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset))
                .Select(date => (dynamic)new
                {
                    GregorianDate = date,
                    ShamsiDate = $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):D2}/{persianCalendar.GetDayOfMonth(date):D2}"
                })
                .ToList();

            var meals = await _NarijeDBContext.Meal.ToListAsync();
            var fileName = $"گزارش تفکیکی بر اساس تاریخ {DateTime.Now:yyyy-MM-dd}.xlsx";
            using (var package = new ExcelPackage())
            {

                CreateMealWorksheet(package, "همه", reserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, showAccessory, accessoryCompanies, company);
                foreach (var meal in meals)
                {
                    var mealReserves = reserves.Where(r => r.MealType == meal.Id).ToList();
                    CreateMealWorksheet(package, meal.Title, mealReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, showAccessory, accessoryCompanies, company);
                }



                var excelBytes = package.GetAsByteArray();
                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        private void CreateMealWorksheet(ExcelPackage package, string mealTitle, List<vReserve> reserves, List<dynamic> shamsiDates, string shamsiPeriodStart, string shamsiPeriodEnd, bool showAccessory, List<AccessoryCompany> accessoryCompanies, string company)
        {
            var worksheet = package.Workbook.Worksheets.Add(mealTitle);
            int totalColumns = shamsiDates.Count + 3;
            totalColumns = totalColumns > 6 ? totalColumns : 7;
            worksheet.View.RightToLeft = true;
            worksheet.Row(1).Height = 100;

            // Header section
            worksheet.Cells[1, 1].Value = company;
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.White);

            var currentDate = DateTime.Now;
            var shamsiDate = $"{new PersianCalendar().GetYear(currentDate)}/{new PersianCalendar().GetMonth(currentDate):D2}/{new PersianCalendar().GetDayOfMonth(currentDate):D2}";
            var currentTime = $"{currentDate:HH:mm}";

            worksheet.Cells[1, totalColumns - 1, 1, totalColumns + 1].Merge = true;
            worksheet.Cells[1, totalColumns - 1].Value = $"تاریخ گزارش‌گیری: {shamsiDate}\n\nساعت گزارش‌گیری: {currentTime}";
            worksheet.Cells[1, totalColumns - 1].Style.Font.Bold = true;
            worksheet.Cells[1, totalColumns - 1].Style.Font.Size = 12;
            worksheet.Cells[1, totalColumns - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[1, totalColumns - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, totalColumns - 1].Style.WrapText = true;
            worksheet.Cells[1, totalColumns - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, totalColumns - 1].Style.Fill.BackgroundColor.SetColor(Color.White);

            worksheet.Cells[1, 4, 1, totalColumns - 2].Merge = true;
            worksheet.Cells[1, 4].Value = "فرم گزارش سفارش مشتریان- تفکیک کالا";
            worksheet.Cells[1, 4].Style.Font.Bold = true;
            worksheet.Cells[1, 4].Style.Font.Size = 16;
            worksheet.Cells[1, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, 4].Style.Fill.BackgroundColor.SetColor(Color.White);

            worksheet.Cells[1, 1, 1, totalColumns + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, 1, totalColumns + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, 1, totalColumns + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, 1, totalColumns + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            totalColumns = shamsiDates.Count + 3;
            // Meal title and date range
            worksheet.Cells[2, 1, 2, 3].Merge = true;
            worksheet.Cells[2, 1].Value = $"  وعده غذایی \n  {mealTitle}";
            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 90;

            int colIndex = 4;
            int rowIndex = 2;
            worksheet.Cells[rowIndex, colIndex].Value = "تاریخ";
            worksheet.Cells[rowIndex, colIndex].Style.TextRotation = 90;
            worksheet.Cells[rowIndex, colIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex, colIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            colIndex++;
            foreach (var date in shamsiDates)
            {
                worksheet.Cells[rowIndex, colIndex].Value = date.ShamsiDate;
                worksheet.Cells[rowIndex, colIndex].Style.TextRotation = 90;
                worksheet.Cells[rowIndex, colIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex, colIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                colIndex++;
            }

            rowIndex++;
            worksheet.Cells[rowIndex, 1, rowIndex, 3].Merge = true;
            worksheet.Cells[rowIndex, 1].Value = $"   از {shamsiPeriodStart} تا {shamsiPeriodEnd}";
            worksheet.Cells[rowIndex, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            int colIndexRow4 = 4;
            worksheet.Cells[rowIndex, colIndexRow4].Value = "جمع غذا در روز";
            worksheet.Cells[rowIndex, colIndexRow4].Style.TextRotation = 90;
            worksheet.Cells[rowIndex, colIndexRow4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex, colIndexRow4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            colIndexRow4++;
         //   var totalAccessoryCount = 0;
          //  if (showAccessory && accessoryCompanies != null)
          //  {
          //      totalAccessoryCount = accessoryCompanies.Sum(ac => ac.Numbers);
          //  }
            foreach (var date in shamsiDates)
            {
                var dateKey = date.GregorianDate.Date;
                worksheet.Cells[rowIndex, colIndexRow4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex, colIndexRow4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                var reserveSum = reserves
                    .Where(r => r.DateTime.Date == dateKey && r.IsFood == true)
                    .Sum(r => r.Num);


                worksheet.Cells[rowIndex, colIndexRow4].Value = reserveSum;//+ totalAccessoryCount;
                colIndexRow4++;
            }

            rowIndex++;
            worksheet.Cells[rowIndex, 1].Value = "نام گروه‌کالا";
            worksheet.Cells[rowIndex, 2].Value = "کدکالا";
            worksheet.Cells[rowIndex, 3].Value = "نام کالا";
            rowIndex++;

            var allFoodTitles = reserves
                .Select(r => r.FoodTitle)
                .Distinct()
                .OrderBy(title => title)
                .ToList();

            if (showAccessory && accessoryCompanies != null)
            {
                var accessoryTitles = accessoryCompanies
                    .Select(ac => ac.Accessory.Title)
                    .Distinct()
                    .OrderBy(title => title)
                    .ToList();
                allFoodTitles.AddRange(accessoryTitles);
            }

            var groupedData = reserves
                .GroupBy(r => r.DateTime.Date)
                .ToDictionary(
                    g => (DateTime)g.Key,
                    g => g.GroupBy(r => r.FoodTitle)
                        .ToDictionary(
                            fg => (string)fg.Key,
                            fg => fg.Sum(r => r.Num))
                );

            if (showAccessory && accessoryCompanies != null)
            {
                foreach (var date in shamsiDates)
                {
                    var dateKey = date.GregorianDate.Date;

                    var accessoryData = accessoryCompanies
                        .GroupBy(ac => ac.Accessory.Title)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Sum(ac => ac.Numbers)
                        );

                    if (groupedData.ContainsKey(dateKey))
                    {
                        foreach (var kvp in accessoryData)
                        {
                            if (groupedData[dateKey].ContainsKey(kvp.Key))
                            {
                                groupedData[dateKey][kvp.Key] += kvp.Value;
                            }
                            else
                            {
                                groupedData[dateKey][kvp.Key] = kvp.Value;
                            }
                        }
                    }
                    else
                    {
                        groupedData[dateKey] = accessoryData;
                    }
                }
            }

            foreach (var foodTitle in allFoodTitles)
            {
                var food = reserves.FirstOrDefault(r => r.FoodTitle == foodTitle);
                var isAccessory = showAccessory && accessoryCompanies != null && accessoryCompanies.Any(ac => ac.Accessory.Title == foodTitle);

                worksheet.Cells[rowIndex, 2].Value = isAccessory
                    ? accessoryCompanies.First(ac => ac.Accessory.Title == foodTitle).Accessory.ArpaNumber
                    : food?.FoodArpaNumber ?? "";
                worksheet.Cells[rowIndex, 1].Value = isAccessory
                    ? "اکسسوری ثابت مشتری"
                    : food?.Category ?? "";

                worksheet.Cells[rowIndex, 3, rowIndex, 4].Merge = true;
                worksheet.Cells[rowIndex, 3].Value = foodTitle;
                worksheet.Cells[rowIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                colIndex = 5;
                int rowSum = 0;
                foreach (var date in shamsiDates)
                {
                    var dateKey = date.GregorianDate.Date;
                    var value = groupedData.ContainsKey(dateKey) && groupedData[dateKey].ContainsKey(foodTitle)
                        ? groupedData[dateKey][foodTitle]
                        : 0;
                    worksheet.Cells[rowIndex, colIndex].Value = value;
                    rowSum += value;
                    colIndex++;
                }
                //    worksheet.Cells[rowIndex, colIndex].Value = rowSum;
                rowIndex++;
            }

            // Apply borders to the entire worksheet
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            for (int i = 1; i <= totalColumns + 1; i++)
            {
                worksheet.Column(i).Width = 15; 
            }

        }
        #endregion





        #region ExportReserveBaseOnTheBranches
        public async Task<FileContentResult> ExportReserveBaseOnTheBranches(DateTime fromDate, DateTime toDate, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var reservesQuery = _NarijeDBContext.vReserves
             .Where(r => r.DateTime.Date >= fromDate.Date && r.DateTime.Date <= toDate.Date && r.Num > 0 );

            if (justPredict)
            {
                reservesQuery= reservesQuery.Where(c => c.State == (int)EnumReserveState.perdict);
            }
            else
            {
                reservesQuery = reservesQuery.Where(c => c.State != (int)EnumReserveState.perdict );
            }



            var company = await _NarijeDBContext.Settings
                                    .Select(A =>
                                     A.CompanyName)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

            List<int> foodGroupIdList = null;
            if (!string.IsNullOrEmpty(foodGroupIds))
            {
                foodGroupIdList = foodGroupIds.Split(',')
                                              .Select(id => int.Parse(id.Trim()))
                                              .ToList();
            }

            if (foodGroupIdList != null && foodGroupIdList.Any())
            {
                reservesQuery = reservesQuery.Where(r => foodGroupIdList.Contains(r.FoodGroupId ?? 0));
            }

            var reserves = await reservesQuery.ToListAsync();

            if (!reserves.Any())
                throw new Exception("در این بازه رزرو یافت نشد");

            List<AccessoryCompany> accessoryCompanies = null;
            if (showAccessory)
            {
                var customerIds = reserves.Select(r => r.CustomerId).Distinct().ToList();
                accessoryCompanies = await _NarijeDBContext.AccessoryCompany
                    .Include(ac => ac.Accessory)
                    .Where(ac => customerIds.Contains(ac.CompanyId))
                    .ToListAsync();
            }

            var persianCalendar = new PersianCalendar();
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toDate)}/{persianCalendar.GetMonth(toDate):D2}/{persianCalendar.GetDayOfMonth(toDate):D2}";

            var shamsiDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset))
                .Select(date => (dynamic)new
                {
                    GregorianDate = date,
                    ShamsiDate = $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):D2}/{persianCalendar.GetDayOfMonth(date):D2}"
                })
                .ToList();

            var meals = await _NarijeDBContext.Meal.ToListAsync();
            var fileName = $"گزارش تفکیکی بر اساس تاریخ {DateTime.Now:yyyy-MM-dd}.xlsx";
            using (var package = new ExcelPackage())
            {

                CreateMealWorksheetForBranches(package, "همه", reserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, showAccessory, accessoryCompanies, company);
                foreach (var meal in meals)
                {
                    var mealReserves = reserves.Where(r => r.MealType == meal.Id).ToList();
                    CreateMealWorksheetForBranches(package, meal.Title, mealReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, showAccessory, accessoryCompanies, company);
                }



                var excelBytes = package.GetAsByteArray();
                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        private void CreateMealWorksheetForBranches(ExcelPackage package, string mealTitle, List<vReserve> reserves, List<dynamic> shamsiDates, string shamsiPeriodStart, string shamsiPeriodEnd, bool showAccessory, List<AccessoryCompany> accessoryCompanies, string companyName)
        {
            var worksheet = package.Workbook.Worksheets.Add(mealTitle);
            int totalColumns = shamsiDates.Count + 3;
            totalColumns = totalColumns > 6 ? totalColumns : 7;
            worksheet.View.RightToLeft = true;
            worksheet.Row(1).Height = 100;

            worksheet.Cells[1, 1].Value = companyName;
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.White);

            var currentDate = DateTime.Now;
            var shamsiDate = $"{new PersianCalendar().GetYear(currentDate)}/{new PersianCalendar().GetMonth(currentDate):D2}/{new PersianCalendar().GetDayOfMonth(currentDate):D2}";
            var currentTime = $"{currentDate:HH:mm}";

            worksheet.Cells[1, totalColumns - 1, 1, totalColumns + 1].Merge = true;
            worksheet.Cells[1, totalColumns - 1].Value = $"تاریخ گزارش‌گیری: {shamsiDate}\n\nساعت گزارش‌گیری: {currentTime}";
            worksheet.Cells[1, totalColumns - 1].Style.Font.Bold = true;
            worksheet.Cells[1, totalColumns - 1].Style.Font.Size = 12;
            worksheet.Cells[1, totalColumns - 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            worksheet.Cells[1, totalColumns - 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, totalColumns - 1].Style.WrapText = true;
            worksheet.Cells[1, totalColumns - 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, totalColumns - 1].Style.Fill.BackgroundColor.SetColor(Color.White);

            worksheet.Cells[1, 4, 1, totalColumns - 2].Merge = true;
            worksheet.Cells[1, 4].Value = "فرم گزارش سفارش مشتریان- تفکیک شعبه خدمات دهنده";
            worksheet.Cells[1, 4].Style.Font.Bold = true;
            worksheet.Cells[1, 4].Style.Font.Size = 16;
            worksheet.Cells[1, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, 4].Style.Fill.BackgroundColor.SetColor(Color.White);

            worksheet.Cells[1, 1, 1, totalColumns + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, 1, totalColumns + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, 1, totalColumns + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, 1, totalColumns + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            totalColumns = shamsiDates.Count + 3;

            worksheet.Cells[2, 1, 2, 3].Merge = true;
            worksheet.Cells[2, 1].Value = $"  وعده غذایی \n  {mealTitle}";
            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 90;

            int colIndex = 4;
            int rowIndex = 2;
            worksheet.Cells[rowIndex, colIndex].Value = "تاریخ";
            worksheet.Cells[rowIndex, colIndex].Style.TextRotation = 90;
            worksheet.Cells[rowIndex, colIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex, colIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            colIndex++;
            foreach (var date in shamsiDates)
            {
                worksheet.Cells[rowIndex, colIndex].Value = date.ShamsiDate;
                worksheet.Cells[rowIndex, colIndex].Style.TextRotation = 90;
                worksheet.Cells[rowIndex, colIndex].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex, colIndex].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                colIndex++;
            }

            rowIndex++;
            worksheet.Cells[rowIndex, 1, rowIndex, 3].Merge = true;
            worksheet.Cells[rowIndex, 1].Value = $"   از {shamsiPeriodStart} تا {shamsiPeriodEnd}";
            worksheet.Cells[rowIndex, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            int colIndexRow4 = 4;
            worksheet.Cells[rowIndex, colIndexRow4].Value = "جمع غذا در روز";
            worksheet.Cells[rowIndex, colIndexRow4].Style.TextRotation = 90;
            worksheet.Cells[rowIndex, colIndexRow4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex, colIndexRow4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            colIndexRow4++;
         //   var totalAccessoryCount = 0;
          //  if (showAccessory && accessoryCompanies != null)
           // {
           //     totalAccessoryCount = accessoryCompanies.Sum(ac => ac.Numbers);
          //  }
            foreach (var date in shamsiDates)
            {
                var dateKey = date.GregorianDate.Date;
                worksheet.Cells[rowIndex, colIndexRow4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex, colIndexRow4].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                var reserveSums = reserves
                    .Where(r => r.DateTime.Date == dateKey && r.IsFood)
                    .Sum(r => r.Num);
                worksheet.Cells[rowIndex, colIndexRow4].Value = reserveSums;//+ totalAccessoryCount;
                colIndexRow4++;
            }

            rowIndex++;
            worksheet.Cells[rowIndex, 1].Value = "نام گروه‌کالا";
            worksheet.Cells[rowIndex, 2].Value = "کدکالا";
            worksheet.Cells[rowIndex, 3].Value = "نام کالا";
            rowIndex++;

            var allFoodTitles = reserves
                .Select(r => r.FoodTitle)
                .Distinct()
                .OrderBy(title => title)
                .ToList();

            if (showAccessory && accessoryCompanies != null)
            {
                var accessoryTitles = accessoryCompanies
                    .Select(ac => ac.Accessory.Title)
                    .Distinct()
                    .OrderBy(title => title)
                    .ToList();
                allFoodTitles.AddRange(accessoryTitles);
            }

            var groupedData = reserves
                .GroupBy(r => r.DateTime.Date)
                .ToDictionary(
                    g => (DateTime)g.Key,
                    g => g.GroupBy(r => r.FoodTitle)
                        .ToDictionary(
                            fg => (string)fg.Key,
                            fg => fg.Sum(r => r.Num))
                );


            var reservesByBranch = reserves
                .GroupBy(r => r.BranchId)
                .ToDictionary(
                    g => g.Key ?? 0,
                    g => g.ToList()
                );


            Dictionary<int, Dictionary<DateTime, List<AccessoryData>>> accessoryData =
        new Dictionary<int, Dictionary<DateTime, List<AccessoryData>>>();

            if (showAccessory && accessoryCompanies != null)
            {
                foreach (var branch in reservesByBranch.Keys)
                {
                    accessoryData[branch] = new Dictionary<DateTime, List<AccessoryData>>();
                    foreach (var date in shamsiDates)
                    {
                        var gregorianDate = date.GregorianDate;
                        accessoryData[branch][gregorianDate] = new List<AccessoryData>();
                    }
                }

                foreach (var ac in accessoryCompanies)
                {
                    var company = ac.Company;
                    var accessory = ac.Accessory;
                    foreach (var date in shamsiDates)
                    {
                        var gregorianDate = date.GregorianDate;
                        var dayOfWeek = gregorianDate.DayOfWeek;
                        int? branchId = null;

                        switch (dayOfWeek)
                        {
                            case DayOfWeek.Saturday:
                                branchId = company.BranchForSaturday;
                                break;
                            case DayOfWeek.Sunday:
                                branchId = company.BranchForSunday;
                                break;
                            case DayOfWeek.Monday:
                                branchId = company.BranchForMonday;
                                break;
                            case DayOfWeek.Tuesday:
                                branchId = company.BranchForTuesday;
                                break;
                            case DayOfWeek.Wednesday:
                                branchId = company.BranchForWednesday;
                                break;
                            case DayOfWeek.Thursday:
                                branchId = company.BranchForThursday;
                                break;
                            case DayOfWeek.Friday:
                                branchId = company.BranchForFriday;
                                break;
                        }

                        if (branchId.HasValue)
                        {
                            int branchKey = branchId.Value;

                            if (!accessoryData.ContainsKey(branchKey))
                            {
                                accessoryData[branchKey] = new Dictionary<DateTime, List<AccessoryData>>();
                            }

                            if (!accessoryData[branchKey].ContainsKey(gregorianDate))
                            {
                                accessoryData[branchKey][gregorianDate] = new List<AccessoryData>();
                            }


                            var existingAccessory = ((List<AccessoryData>)accessoryData[branchKey][gregorianDate])
                                 .FirstOrDefault(a => a.FoodTitle == accessory.Title);

                            if (existingAccessory != null)
                            {
                                existingAccessory.Quantity += ac.Numbers;
                            }
                            else
                            {
                                accessoryData[branchKey][gregorianDate].Add(new AccessoryData
                                {
                                    FoodTitle = accessory.Title,
                                    Quantity = ac.Numbers,
                                    ArpaNumber = accessory.ArpaNumber,
                                });
                            }
                        }
                    }
                }
            }



            var colors = new[] { Color.LightGreen, Color.Yellow, Color.LightBlue };
            var indexColor = 0;
            foreach (var branch in reservesByBranch)
            {
                int branchId = branch.Key;
                var branchReserves = branch.Value;


                worksheet.Cells[rowIndex, 1, rowIndex, 4].Merge = true;
                worksheet.Cells[rowIndex, 1].Value = $"{branchReserves.First().BranchTitle},  جمع غذا ها =>";
                worksheet.Cells[rowIndex, 1].Style.Font.Bold = true;
                worksheet.Cells[rowIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[rowIndex, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                var backgroundColor = colors[indexColor % colors.Length];
                worksheet.Cells[rowIndex, 1, rowIndex, shamsiDates.Count + 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[rowIndex, 1, rowIndex, shamsiDates.Count + 4].Style.Fill.BackgroundColor.SetColor(backgroundColor);
                indexColor++;


                int colIndexs = 5;
                foreach (var date in shamsiDates)
                {
                    var dateKey = date.GregorianDate.Date;
                    int total = branchReserves
                        .Where(r => r.DateTime.Date == dateKey && r.IsFood == true)
                        .Sum(r => r.Num);

                   // if (showAccessory && accessoryData.TryGetValue(branchId, out var branchAccessoryDataForDate))
                  //  {
                   //     if (branchAccessoryDataForDate.TryGetValue(dateKey, out List<AccessoryData> dateData))
                    //    {
                         //   total += dateData.Sum(a => a.Quantity);
                    //    }
                   // }

                    worksheet.Cells[rowIndex, colIndexs].Value = total;
                    colIndexs++;
                }
                rowIndex++;


                foreach (var foodGroup in branchReserves.GroupBy(r => r.FoodTitle))
                {
                    worksheet.Cells[rowIndex, 2].Value = $"{foodGroup.First().FoodArpaNumber}";
                    worksheet.Cells[rowIndex, 1].Value = $"{foodGroup.First().Category}";
                    worksheet.Cells[rowIndex, 3].Value = foodGroup.Key;
                    worksheet.Cells[rowIndex, 3, rowIndex, 4].Merge = true;
                    int colIndexReserve = 5;
                    foreach (var date in shamsiDates)
                    {
                        var dateKey = date.GregorianDate.Date;
                        var foodSum = foodGroup.Where(r => r.DateTime.Date == dateKey).Sum(r => r.Num);
                        worksheet.Cells[rowIndex, colIndexReserve].Value = foodSum;
                        colIndexReserve++;
                    }
                    rowIndex++;
                }


                if (showAccessory && accessoryData.TryGetValue(branchId, out var branchAccessoryDataForAccessory))
                {
                    var accessoriesByTitle = branchAccessoryDataForAccessory
                        .SelectMany(d => d.Value)
                        .GroupBy(a => a.FoodTitle)
                        .ToDictionary(
                            g => g.Key,
                            g => new
                            {
                                Total = g.Sum(a => a.Quantity),
                                ArpaNumber = g.Select(c => c.ArpaNumber),
                            });

                    foreach (var accessory in accessoriesByTitle)
                    {
                        worksheet.Cells[rowIndex, 1].Value = "اکسسوری ثابت شرکت";
                        worksheet.Cells[rowIndex, 2].Value = accessory.Value.ArpaNumber;
                        worksheet.Cells[rowIndex, 3].Value = accessory.Key;
                        worksheet.Cells[rowIndex, 3, rowIndex, 4].Merge = true;

                        int colIndexAccessory = 5;
                        foreach (var date in shamsiDates)
                        {
                            var gregorianDate = date.GregorianDate;
                            int quantity = 0;
                            if (branchAccessoryDataForAccessory.TryGetValue(gregorianDate, out List<AccessoryData> dateData))
                            {
                                quantity = dateData
                                    .Where(a => a.FoodTitle == accessory.Key)
                                    .Sum(a => a.Quantity);
                            }
                            worksheet.Cells[rowIndex, colIndexAccessory].Value = quantity;
                            colIndexAccessory++;
                        }
                        rowIndex++;
                    }
                }
            }







            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            for (int i = 1; i <= totalColumns + 1; i++)
            {
                worksheet.Column(i).Width = 15;
            }

        }
        #endregion




        #region ExportReserveBaseOnTheCustomers
        private string GetPersianDayName(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Saturday:
                    return "شنبه";
                case DayOfWeek.Sunday:
                    return "یک‌شنبه";
                case DayOfWeek.Monday:
                    return "دوشنبه";
                case DayOfWeek.Tuesday:
                    return "سه‌شنبه";
                case DayOfWeek.Wednesday:
                    return "چهارشنبه";
                case DayOfWeek.Thursday:
                    return "پنج‌شنبه";
                case DayOfWeek.Friday:
                    return "جمعه";
                default:
                    return string.Empty;
            }
        }

        public async Task<FileContentResult> ExportReserveBaseOnTheCustomers(DateTime fromDate, DateTime toDate, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false, bool isPdf = false)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var reservesQuery = _NarijeDBContext.vReserves
                   .Where(r => r.DateTime.Date >= fromDate.Date && r.DateTime.Date <= toDate.Date && r.Num > 0);

            if (justPredict)
            {
                reservesQuery = reservesQuery.Where(c => c.State == (int)EnumReserveState.perdict);
            }
            else
            {
                reservesQuery = reservesQuery.Where(c => c.State != (int)EnumReserveState.perdict);
            }

            var company = await _NarijeDBContext.Settings
                                    .Select(A =>
                                     A.CompanyName)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

            List<int> foodGroupIdList = null;
            if (!string.IsNullOrEmpty(foodGroupIds))
            {
                foodGroupIdList = foodGroupIds.Split(',')
                                              .Select(id => int.Parse(id.Trim()))
                                              .ToList();
            }

            if (foodGroupIdList != null && foodGroupIdList.Any())
            {
                reservesQuery = reservesQuery.Where(r => foodGroupIdList.Contains(r.FoodGroupId ?? 0));
            }

            var reserves = await reservesQuery.ToListAsync();

            // Fetch all active customers to display, even if they have zero reserves
            var activeCustomers = await _NarijeDBContext.Customers
                .Where(c => c.Active)
                .AsNoTracking()
                .ToListAsync();

            // Build Branch title dictionary
            var branchTitleMap = await _NarijeDBContext.Branch
                .AsNoTracking()
                .ToDictionaryAsync(b => b.Id, b => b.Title);

            // Map each customer to the service branch for the specific Persian day
            var persianCalendar = new PersianCalendar();
            var dayOfWeek = persianCalendar.GetDayOfWeek(fromDate);
            var customerBranchMap = new Dictionary<int, int>();
            foreach (var c in activeCustomers)
            {
                int? branchIdForDay = null;
                switch (dayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        branchIdForDay = c.BranchForSaturday; break;
                    case DayOfWeek.Sunday:
                        branchIdForDay = c.BranchForSunday; break;
                    case DayOfWeek.Monday:
                        branchIdForDay = c.BranchForMonday; break;
                    case DayOfWeek.Tuesday:
                        branchIdForDay = c.BranchForTuesday; break;
                    case DayOfWeek.Wednesday:
                        branchIdForDay = c.BranchForWednesday; break;
                    case DayOfWeek.Thursday:
                        branchIdForDay = c.BranchForThursday; break;
                    case DayOfWeek.Friday:
                        branchIdForDay = c.BranchForFriday; break;
                }
                customerBranchMap[c.Id] = branchIdForDay ?? 0;
            }

            // Accessory companies for all involved customers
            List<AccessoryCompany> accessoryCompanies = null;
            if (showAccessory)
            {
                var customerIds = activeCustomers.Select(c => c.Id).ToList();
                accessoryCompanies = await _NarijeDBContext.AccessoryCompany
                    .Include(ac => ac.Accessory)
                    .Where(ac => customerIds.Contains(ac.CompanyId))
                    .ToListAsync();
            }

            if (isPdf)
            {
                var pdfBytes = CreateCustomersPdf(activeCustomers, customerBranchMap, branchTitleMap, reserves, accessoryCompanies, showAccessory, fromDate, company);
                return new FileContentResult(pdfBytes, "application/pdf")
                {
                    FileDownloadName = $"گزارش تفکیکی بر اساس مشتریان {DateTime.Now:yyyy-MM-dd}.pdf"
                };
            }

            var shamsiDate = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
            var persianDayName = GetPersianDayName(dayOfWeek);
            var meals = await _NarijeDBContext.Meal.ToListAsync();

            var dateWithDayName = $"{shamsiDate} {persianDayName}";
            var fileName = $"گزارش تفکیکی بر اساس مشتریان {DateTime.Now:yyyy-MM-dd}.xlsx";
            using (var package = new ExcelPackage())
            {
                // Main consolidated sheet
                CreateMealWorksheetForCustomers(package, "لیست غذایی", reserves, dateWithDayName, activeCustomers, customerBranchMap, branchTitleMap, showAccessory, accessoryCompanies, company);
                foreach (var meal in meals)
                {
                    var mealReserves = reserves.Where(r => r.MealType == meal.Id).ToList();
                    CreateMealWorksheetForCustomers(package, meal.Title, mealReserves, dateWithDayName, activeCustomers, customerBranchMap, branchTitleMap, showAccessory, accessoryCompanies, company);
                }

                var excelBytes = package.GetAsByteArray();
                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        private static string FormatDeliverHour(string deliverHour)
        {
            if (string.IsNullOrWhiteSpace(deliverHour)) return string.Empty;
            if (TimeSpan.TryParse(deliverHour, out var ts))
                return new DateTime(ts.Ticks).ToString("HH:mm");
            // fallback trims seconds if present 00:00:00 -> 00:00
            if (deliverHour.Length >= 5) return deliverHour.Substring(0, 5);
            return deliverHour;
        }

        private void CreateMealWorksheetForCustomers(ExcelPackage package, string mealTitle, List<vReserve> reserves, string dateWithDayName, List<Customer> activeCustomers, Dictionary<int, int> customerBranchMap, Dictionary<int, string> branchTitleMap, bool showAccessory, List<AccessoryCompany> accessoryCompanies, string company)
        {
            // Build food set
            var allFoods = reserves
                .Select(r => new { Title = r.FoodTitle, Arpa = r.FoodArpaNumber, Category = r.Category, IsFood = r.IsFood })
                .Distinct()
                .ToList();

            if (showAccessory && accessoryCompanies != null)
            {
                var accessories = accessoryCompanies
                    .Select(ac => new { Title = ac.Accessory.Title, Arpa = ac.Accessory.ArpaNumber, Category = "اکسسوری", IsFood = false })
                    .Distinct()
                    .ToList();
                // Union by title to avoid duplicates
                allFoods = allFoods.Concat(accessories)
                    .GroupBy(f => f.Title)
                    .Select(g => g.First())
                    .ToList();
            }

            // Determine which titles are main foods
            var isMainFoodByTitle = allFoods
                .ToDictionary(f => f.Title, f => reserves.Any(r => r.FoodTitle == f.Title && r.IsFood));

            var worksheet = package.Workbook.Worksheets.Add(mealTitle);

            // Page setup
            worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
            worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;
            worksheet.PrinterSettings.FitToHeight = 0;

            var reservesCount = allFoods.Count();
            int preColumns = 6; // Service Branch, CustomerName, BranchCode, BranchTitle, DeliverTime, Total
            int totalColumns = reservesCount + preColumns;

            worksheet.View.RightToLeft = true;
            worksheet.Row(1).Height = 60;

            worksheet.Cells[1, 1].Value = company + " , " + "فرم گزارش سفارش روزانه مشتریان";
            worksheet.Cells[1, 1, 1, preColumns].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.White);

            var currentDate = DateTime.Now;
            var pc = new PersianCalendar();
            var shamsiDate = $"{pc.GetYear(currentDate)}/{pc.GetMonth(currentDate):D2}/{pc.GetDayOfMonth(currentDate):D2}";
            var currentTime = $"{currentDate:HH:mm}";
            var dateRange = worksheet.Cells[1, preColumns + 1, 1, totalColumns];
            dateRange.Merge = true;
            dateRange.Value = $"تاریخ گزارش‌گیری: {shamsiDate}\n\nساعت گزارش‌گیری: {currentTime}";
            dateRange.Style.Font.Bold = true;
            dateRange.Style.Font.Size = 12;
            dateRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            dateRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            dateRange.Style.WrapText = true;
            dateRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            dateRange.Style.Fill.BackgroundColor.SetColor(Color.White);

            worksheet.Cells[1, 1, 1, totalColumns].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            worksheet.Cells[2, 1, 2, preColumns].Merge = true;
            worksheet.Cells[2, 1].Value = $"  وعده غذایی \n  {mealTitle}";
            worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
            worksheet.Row(2).Height = 60;

            // Header for food titles
            int colIndex = preColumns + 1;
            worksheet.Cells[2, colIndex].Value = "لیست غذایی";
            worksheet.Cells[2, colIndex, 2, totalColumns].Merge = true;
            worksheet.Cells[2, colIndex, 2, totalColumns].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, colIndex, 2, totalColumns].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Row 3: Food Names
            for (int i = 0; i < allFoods.Count; i++)
            {
                var f = allFoods[i];
                worksheet.Cells[3, preColumns + 1 + i].Value = f.Title;
                worksheet.Cells[3, preColumns + 1 + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, preColumns + 1 + i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[3, preColumns + 1 + i].Style.WrapText = true;
                // Color only main foods columns
                if (isMainFoodByTitle.TryGetValue(f.Title, out var main) && main)
                {
                    worksheet.Cells[3, preColumns + 1 + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[3, preColumns + 1 + i].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                }
            }

            // Row 4: Food Codes rotated 90 degrees
            colIndex = preColumns + 1;
            for (int i = 0; i < allFoods.Count; i++)
            {
                worksheet.Cells[4, colIndex + i].Value = allFoods[i].Arpa;
                worksheet.Cells[4, colIndex + i].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[4, colIndex + i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[4, colIndex + i].Style.TextRotation = 90;
            }

            int totalRowsForMerge = 4 + activeCustomers.Count;

            worksheet.Cells[3, 1, totalRowsForMerge, preColumns].Merge = true;
            worksheet.Cells[3, 1].Value = $"   تاریخ ارایه غذا: \n\r {dateWithDayName}";
            worksheet.Cells[3, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            worksheet.Cells[3, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

            // Row 4, preColumns: label for totals column
            worksheet.Cells[4, preColumns].Value = "جمع کل";
            worksheet.Cells[4, preColumns].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[4, preColumns].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Row 4: totals per food across all customers
            colIndex = preColumns + 1;
            for (int i = 0; i < allFoods.Count; i++)
            {
                var foodTitle = allFoods[i].Title;
                int total = reserves.Where(r => r.FoodTitle == foodTitle).Sum(r => r.Num);
                if (showAccessory && (allFoods[i].Category == "اکسسوری"))
                {
                    var accessoryTotal = accessoryCompanies?.Where(ac => ac.Accessory.Title == foodTitle).Sum(ac => ac.Numbers) ?? 0;
                    total += accessoryTotal;
                }
                worksheet.Cells[4, colIndex + i].Value = total == 0 ? (object)string.Empty : total;
            }

            // Body header
            int rowIndex = 5;
            worksheet.Cells[rowIndex, 1].Value = "شعبه خدمات دهنده";
            worksheet.Cells[rowIndex, 2].Value = "نام مشتری";
            worksheet.Cells[rowIndex, 3].Value = "کد شعبه";
            worksheet.Cells[rowIndex, 4].Value = "نام شعبه";
            worksheet.Cells[rowIndex, 5].Value = "ساعت تحویل";
            worksheet.Cells[rowIndex, 6].Value = "جمع کل";
            rowIndex++;

            // Build customer display entries with stable ordering (ParentId, ParentTitle, Title)
            var customersById = activeCustomers.ToDictionary(c => c.Id, c => c);
            string GetParentTitle(Customer c)
            {
                if (c.ParentId.HasValue && customersById.TryGetValue(c.ParentId.Value, out var p))
                    return p.Title;
                return string.Empty;
            }

            var displayCustomers = activeCustomers
                .Select(c => new
                {
                    Customer = c,
                    ParentTitle = GetParentTitle(c),
                    BranchId = customerBranchMap.TryGetValue(c.Id, out var bid) ? bid : 0,
                    BranchTitle = (customerBranchMap.TryGetValue(c.Id, out var bid2) && branchTitleMap.ContainsKey(bid2)) ? branchTitleMap[bid2] : string.Empty
                })
                .OrderBy(x => x.Customer.ParentId ?? 0)
                .ThenBy(x => x.ParentTitle)
                .ThenBy(x => x.Customer.Title)
                .ToList();

            // Group by branch title for display
            var branchGroups = displayCustomers
                .GroupBy(x => new { x.BranchId, x.BranchTitle })
                .OrderBy(g => g.Key.BranchTitle)
                .ToList();

            foreach (var branchGroup in branchGroups)
            {
                int branchStartRow = rowIndex;

                foreach (var item in branchGroup)
                {
                    var c = item.Customer;
                    var customerReserves = reserves.Where(r => r.CustomerId == c.Id).ToList();

                    worksheet.Cells[rowIndex, 1].Value = branchGroup.Key.BranchTitle;
                    worksheet.Cells[rowIndex, 2].Value = string.IsNullOrWhiteSpace(item.ParentTitle) ? c.Title : ($"{c.Title} - {item.ParentTitle}");
                    worksheet.Cells[rowIndex, 3].Value = branchGroup.Key.BranchId == 0 ? string.Empty : branchGroup.Key.BranchId.ToString();
                    worksheet.Cells[rowIndex, 4].Value = branchGroup.Key.BranchTitle;

                    var deliver = customerReserves.FirstOrDefault()?.DeliverHour ?? string.Empty;
                    worksheet.Cells[rowIndex, 5].Value = FormatDeliverHour(deliver);

                    // Grand total: main foods + accessories
                    int grandTotalFoods = customerReserves.Sum(r => r.Num);
                    int grandAccessory = 0;
                    if (showAccessory && accessoryCompanies != null)
                    {
                        grandAccessory = accessoryCompanies.Where(ac => ac.CompanyId == c.Id).Sum(ac => ac.Numbers);
                    }
                    worksheet.Cells[rowIndex, 6].Value = (grandTotalFoods + grandAccessory);

                    // Per food columns
                    colIndex = preColumns + 1;
                    for (int i = 0; i < allFoods.Count; i++)
                    {
                        var food = allFoods[i];
                        int foodTotal = customerReserves.Where(r => r.FoodTitle == food.Title).Sum(r => r.Num);
                        if (showAccessory && food.Category == "اکسسوری")
                        {
                            var accessoryTotal = accessoryCompanies?.Where(ac => ac.CompanyId == c.Id && ac.Accessory.Title == food.Title).Sum(ac => ac.Numbers) ?? 0;
                            foodTotal += accessoryTotal;
                        }
                        if (foodTotal == 0)
                        {
                            worksheet.Cells[rowIndex, colIndex + i].Value = string.Empty;
                            worksheet.Cells[rowIndex, colIndex + i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[rowIndex, colIndex + i].Style.Fill.BackgroundColor.SetColor(Color.White);
                        }
                        else
                        {
                            worksheet.Cells[rowIndex, colIndex + i].Value = foodTotal;
                        }
                    }

                    rowIndex++;
                }

                if (branchStartRow < rowIndex - 1)
                {
                    worksheet.Cells[branchStartRow, 1, rowIndex - 1, 1].Merge = true;
                    worksheet.Cells[branchStartRow, 1, rowIndex - 1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
            }

            // Borders and alignment
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1, rowIndex - 1, totalColumns].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Column widths: pre columns wider, food columns narrower
            for (int i = 1; i <= preColumns; i++) worksheet.Column(i).Width = 20;
            for (int i = preColumns + 1; i <= totalColumns; i++) worksheet.Column(i).Width = 10;

            // Make header fonts bold
            worksheet.Cells[1, 1, 4, totalColumns].Style.Font.Bold = true;
        }

        private byte[] CreateCustomersPdf(List<Customer> activeCustomers, Dictionary<int, int> customerBranchMap, Dictionary<int, string> branchTitleMap, List<vReserve> reserves, List<AccessoryCompany> accessoryCompanies, bool showAccessory, DateTime date, string company)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var pc = new PersianCalendar();
            var shamsiDate = $"{pc.GetYear(date)}/{pc.GetMonth(date):D2}/{pc.GetDayOfMonth(date):D2}";

            var customersById = activeCustomers.ToDictionary(c => c.Id, c => c);
            string GetParentTitle(Customer c)
            {
                if (c.ParentId.HasValue && customersById.TryGetValue(c.ParentId.Value, out var p))
                    return p.Title;
                return string.Empty;
            }

            // Food columns (titles) based on reserves
            var foodTitles = reserves.Select(r => r.FoodTitle).Distinct().OrderBy(t => t).ToList();

            var displayCustomers = activeCustomers
                .Select(c => new
                {
                    Customer = c,
                    ParentTitle = GetParentTitle(c),
                    BranchId = customerBranchMap.TryGetValue(c.Id, out var bid) ? bid : 0,
                    BranchTitle = (customerBranchMap.TryGetValue(c.Id, out var bid2) && branchTitleMap.ContainsKey(bid2)) ? branchTitleMap[bid2] : string.Empty
                })
                .OrderBy(x => x.Customer.ParentId ?? 0)
                .ThenBy(x => x.ParentTitle)
                .ThenBy(x => x.Customer.Title)
                .ToList();

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.Header().Text($"{company} - گزارش مشتریان ({shamsiDate})").SemiBold().FontSize(14).AlignRight();

                    page.Content().Table(table =>
                    {
                        // Columns: Customer, Total, Foods...
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.ConstantColumn(60);
                            foreach (var _ in foodTitles)
                                columns.ConstantColumn(40);
                        });

                        // Header row
                        table.Header(header =>
                        {
                            header.Cell().Text("نام مشتری").SemiBold();
                            header.Cell().Text("جمع کل").SemiBold();
                            foreach (var ft in foodTitles)
                                header.Cell().Text(ft).SemiBold();
                        });

                        // Rows
                        foreach (var item in displayCustomers)
                        {
                            var c = item.Customer;
                            var customerReserves = reserves.Where(r => r.CustomerId == c.Id).ToList();
                            var customerTitle = string.IsNullOrWhiteSpace(item.ParentTitle) ? c.Title : ($"{c.Title} - {item.ParentTitle}");
                            int grandFood = customerReserves.Sum(r => r.Num);
                            int grandAcc = showAccessory && accessoryCompanies != null ? accessoryCompanies.Where(ac => ac.CompanyId == c.Id).Sum(ac => ac.Numbers) : 0;

                            table.Cell().Text(customerTitle);
                            table.Cell().Text((grandFood + grandAcc).ToString());
                            foreach (var ft in foodTitles)
                            {
                                var cnt = customerReserves.Where(r => r.FoodTitle == ft).Sum(r => r.Num);
                                table.Cell().Text(cnt == 0 ? string.Empty : cnt.ToString());
                            }
                        }
                    });
                });
            });

            return doc.GeneratePdf();
        }
        #endregion


        #region ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheFood
        public async Task<FileContentResult> ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheFood(DateTime fromDate, DateTime toDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var allReservesQuery = _NarijeDBContext.vReserves
                .Where(r => r.DateTime.Date >= fromDate.Date && r.DateTime.Date <= toDate.Date && r.Num > 0);

            var allReserves = await allReservesQuery.ToListAsync();

            if (!allReserves.Any())
                throw new Exception("در این بازه رزرو یافت نشد");

            var normalReserves = allReserves.Where(r => r.State != (int)EnumReserveState.perdict).ToList();
            var predictReserves = allReserves.Where(r => r.State == (int)EnumReserveState.perdict).ToList();

            var company = await _NarijeDBContext.Settings
                .Select(A => A.CompanyName)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var persianCalendar = new PersianCalendar();
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toDate)}/{persianCalendar.GetMonth(toDate):D2}/{persianCalendar.GetDayOfMonth(toDate):D2}";

            var shamsiDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset))
                .Select(date => (dynamic)new
                {
                    GregorianDate = date,
                    ShamsiDate = $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):D2}/{persianCalendar.GetDayOfMonth(date):D2}"
                })
                .ToList();

            var meals = await _NarijeDBContext.Meal.ToListAsync();
            var fileName = $"گزارش تفاوت بین پیش‌بینی و رزرو عادی بر اساس تاریخ {DateTime.Now:yyyy-MM-dd}.xlsx";
            using (var package = new ExcelPackage())
            {
                CreateDifferenceWorksheet(package, "همه", normalReserves, predictReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, company);
                foreach (var meal in meals)
                {
                    var mealNormalReserves = normalReserves.Where(r => r.MealType == meal.Id).ToList();
                    var mealPredictReserves = predictReserves.Where(r => r.MealType == meal.Id).ToList();
                    CreateDifferenceWorksheet(package, meal.Title, mealNormalReserves, mealPredictReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, company);
                }

                var excelBytes = package.GetAsByteArray();
                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        private void CreateDifferenceWorksheet(ExcelPackage package, string mealTitle, List<vReserve> normalReserves, List<vReserve> predictReserves, List<dynamic> shamsiDates, string shamsiPeriodStart, string shamsiPeriodEnd, string company)
        {
            var worksheet = package.Workbook.Worksheets.Add(mealTitle);
            worksheet.View.RightToLeft = true;

            worksheet.Cells[1, 1].Value = "گزارش مغایرت سفارش مشتری با مقدار پیش بینی ( به تفکیک کالا )";
            worksheet.Cells[1, 1, 1, 7].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells[2, 1].Value = $"بازه زمانی از {shamsiPeriodStart} لغایت {shamsiPeriodEnd}";
            worksheet.Cells[2, 1, 2, 7].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Bold = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            string[] headers = { "تاریخ", "کد کالا", "نام کالا", "مقدار پیش بینی", "مقدار سفارش مشتری", "اختلاف سفارش مشتری از مقدارپیش بینی", "درصد اختلاف" };
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
            }

            var groupedNormal = normalReserves
                .GroupBy(r => new { Date = r.DateTime.Date, r.FoodTitle })
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Num));

            var groupedPredict = predictReserves
                .GroupBy(r => new { Date = r.DateTime.Date, r.FoodTitle })
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Num));

            var allFoods = normalReserves.Select(r => new { r.FoodTitle, r.FoodArpaNumber, r.Category })
                                .Concat(predictReserves.Select(r => new { r.FoodTitle, r.FoodArpaNumber, r.Category }))
                                .Distinct()
                                .OrderBy(f => f.FoodTitle)
                                .ToList();

            int currentRow = 4;
            foreach (var date in shamsiDates.OrderBy(d => d.GregorianDate))
            {
                DateTime targetDate = date.GregorianDate.Date;
                foreach (var food in allFoods)
                {
                    var key = new { Date = targetDate, food.FoodTitle };

                    bool hasNormal = groupedNormal.TryGetValue(key, out int normalValue);
                    bool hasPredict = groupedPredict.TryGetValue(key, out int predictValue);

                    if (!hasNormal && !hasPredict)
                    {
                        continue; 
                    }

                    normalValue = hasNormal ? normalValue : 0;
                    predictValue = hasPredict ? predictValue : 0;
                    var difference = normalValue - predictValue;
                    var percentage = (double)difference / predictValue ;

                    string formattedDifference = difference > 0 ? $"+{difference}" : difference.ToString();
                    string formattedPercentage = predictValue > 0 ?  percentage > 0 ? $"+{percentage:P2}" : percentage < 0 ? percentage.ToString("P2") : "0%" :"-100%";

                    // Populate row
                    worksheet.Cells[currentRow, 1].Value = date.ShamsiDate;
                    worksheet.Cells[currentRow, 2].Value = food.FoodArpaNumber;
                    worksheet.Cells[currentRow, 3].Value = food.FoodTitle;
                    worksheet.Cells[currentRow, 4].Value = predictValue;
                    worksheet.Cells[currentRow, 5].Value = normalValue;
                    worksheet.Cells[currentRow, 6].Value = formattedDifference;
                    worksheet.Cells[currentRow, 7].Value = formattedPercentage;

                    currentRow++;
                }
            }

            var allCells = worksheet.Cells[1, 1, currentRow - 1, 7];
            allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            allCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            for (int col = 1; col <= 7; col++)
            {
                worksheet.Column(col).Width = 20; 
            }
        }
        #endregion



        #region ExportDifferenceBetweenPredictAndNormalReserveBaseOnTheBranches
        public async Task<FileContentResult> ExportDifferenceBetweenPredictAndNormalReserveBaseOnTheBranches(DateTime fromDate, DateTime toDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var allReservesQuery = _NarijeDBContext.vReserves
                .Where(r => r.DateTime.Date >= fromDate.Date && r.DateTime.Date <= toDate.Date && r.Num > 0);

            var allReserves = await allReservesQuery.ToListAsync();

            if (!allReserves.Any())
                throw new Exception("در این بازه رزرو یافت نشد");

            var normalReserves = allReserves.Where(r => r.State != (int)EnumReserveState.perdict).ToList();
            var predictReserves = allReserves.Where(r => r.State == (int)EnumReserveState.perdict).ToList();

            var company = await _NarijeDBContext.Settings
                .Select(A => A.CompanyName)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var persianCalendar = new PersianCalendar();
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toDate)}/{persianCalendar.GetMonth(toDate):D2}/{persianCalendar.GetDayOfMonth(toDate):D2}";


            var shamsiDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset))
                .Select(date => (dynamic)new
                {
                    GregorianDate = date,
                    ShamsiDate = $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):D2}/{persianCalendar.GetDayOfMonth(date):D2}"
                })
                .ToList();
            var meals = await _NarijeDBContext.Meal.ToListAsync();
            var fileName = $"گزارش تفاوت بین پیش‌بینی و رزرو عادی بر اساس تاریخ {DateTime.Now:yyyy-MM-dd}.xlsx";
            using (var package = new ExcelPackage())
            {
                CreateBranchDifferenceWorksheet(package, "همه", normalReserves, predictReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, company);
                foreach (var meal in meals)
                {
                    var mealNormalReserves = normalReserves.Where(r => r.MealType == meal.Id).ToList();
                    var mealPredictReserves = predictReserves.Where(r => r.MealType == meal.Id).ToList();
                    CreateBranchDifferenceWorksheet(package, meal.Title, mealNormalReserves, mealPredictReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, company);
                }

                var excelBytes = package.GetAsByteArray();
                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        private void CreateBranchDifferenceWorksheet(ExcelPackage package, string sheetTitle, List<vReserve> normalReserves, List<vReserve> predictReserves, List<dynamic> shamsiDates, string shamsiPeriodStart, string shamsiPeriodEnd, string company)
        {
            var worksheet = package.Workbook.Worksheets.Add(sheetTitle);
            worksheet.View.RightToLeft = true;

            // Header Row 1
            worksheet.Cells[1, 1].Value = "گزارش مغایرت سفارش مشتری با مقدار پیش بینی ( به تفکیک شعبه )";
            worksheet.Cells[1, 1, 1, 7].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Header Row 2
            worksheet.Cells[2, 1].Value = $"بازه زمانی از {shamsiPeriodStart} لغایت {shamsiPeriodEnd}";
            worksheet.Cells[2, 1, 2, 7].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Bold = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Column Headers
            string[] headers = {
        "تاریخ",
        "نام شعبه خدمات دهنده",
        "کد کالا",
        "نام کالا",
        "مقدار پیش بینی",
        "مقدار سفارش مشتری",
        "درصد اختلاف"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
            }

            // Group reserves by branch
            var branchGroups = normalReserves
                .Select(r => r.BranchId ?? 0)
                .Concat(predictReserves.Select(r => r.BranchId ?? 0))
                .Distinct()
                .Select(branchId => new
                {
                    BranchId = branchId,
                    BranchTitle = normalReserves.FirstOrDefault(r => r.BranchId == branchId)?.BranchTitle ??
                                  predictReserves.FirstOrDefault(r => r.BranchId == branchId)?.BranchTitle ?? "نامشخص",
                    NormalReserves = normalReserves.Where(r => r.BranchId == branchId).ToList(),
                    PredictReserves = predictReserves.Where(r => r.BranchId == branchId).ToList()
                })
                .OrderBy(b => b.BranchTitle)
                .ToList();

            // Define background colors for branch headers
            var branchColors = new List<Color>
    {
        Color.FromArgb(255, 230, 230), // Light red
        Color.FromArgb(230, 255, 230), // Light green
        Color.FromArgb(230, 230, 255), // Light blue
        Color.FromArgb(255, 255, 230), // Light yellow
        Color.FromArgb(255, 230, 255)  // Light purple
    };

            int currentRow = 4;
            int colorIndex = 0;

            foreach (var branch in branchGroups)
            {
                // Add branch header row with background color
                worksheet.Cells[currentRow, 1, currentRow, 7].Merge = true;
                worksheet.Cells[currentRow, 1].Value = branch.BranchTitle;
                worksheet.Cells[currentRow, 1].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[currentRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[currentRow, 1, currentRow, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[currentRow, 1, currentRow, 7].Style.Fill.BackgroundColor.SetColor(branchColors[colorIndex % branchColors.Count]);
                currentRow++;
                colorIndex++;

                // Group by date and food item
                var allDates = branch.NormalReserves.Select(r => r.DateTime.Date)
                    .Concat(branch.PredictReserves.Select(r => r.DateTime.Date))
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();

                foreach (var date in allDates)
                {
                    var dateInfo = shamsiDates.FirstOrDefault(d => d.GregorianDate.Date == date);
                    if (dateInfo == null) continue;

                    // Get all food items for this date and branch
                    var allFoods = branch.NormalReserves.Where(r => r.DateTime.Date == date)
                        .Select(r => new { r.FoodArpaNumber, r.FoodTitle, r.Category })
                        .Concat(branch.PredictReserves.Where(r => r.DateTime.Date == date)
                            .Select(r => new { r.FoodArpaNumber, r.FoodTitle, r.Category }))
                        .Distinct()
                        .OrderBy(f => f.FoodTitle)
                        .ToList();

                    foreach (var food in allFoods)
                    {
                        var normalValue = branch.NormalReserves
                            .Where(r => r.DateTime.Date == date && r.FoodArpaNumber == food.FoodArpaNumber && r.FoodTitle == food.FoodTitle)
                            .Sum(r => r.Num);

                        var predictValue = branch.PredictReserves
                            .Where(r => r.DateTime.Date == date && r.FoodArpaNumber == food.FoodArpaNumber && r.FoodTitle == food.FoodTitle)
                            .Sum(r => r.Num);

                        // Skip if both values are zero
                        if (normalValue == 0 && predictValue == 0)
                            continue;

                        var difference = normalValue - predictValue;
                        var percentage = predictValue != 0 ? (double)difference / predictValue : 0;

                        // Format values with +/- signs
                        string formattedDifference = difference > 0 ? $"+{difference}" : difference.ToString();
                        string formattedPercentage = predictValue > 0 ? percentage > 0 ? $"+{percentage:P2}" : percentage < 0 ? percentage.ToString("P2") : "0%" : "-100%";


                        // Populate row
                        worksheet.Cells[currentRow, 1].Value = dateInfo.ShamsiDate;
                        worksheet.Cells[currentRow, 2].Value = branch.BranchTitle;
                        worksheet.Cells[currentRow, 3].Value = food.FoodArpaNumber;
                        worksheet.Cells[currentRow, 4].Value = food.FoodTitle;
                        worksheet.Cells[currentRow, 5].Value = predictValue;
                        worksheet.Cells[currentRow, 6].Value = normalValue;
                        worksheet.Cells[currentRow, 7].Value = formattedPercentage;

                        currentRow++;
                    }
                }
            }

            // Apply formatting to all cells
            if (currentRow > 4)
            {
                var allCells = worksheet.Cells[1, 1, currentRow - 1, 7];
                allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                allCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                // Set column widths
                for (int col = 1; col <= 7; col++)
                {
                    worksheet.Column(col).Width = 20; // ~100px
                }
            }
            else
            {
                // Add message if no data found
                worksheet.Cells[4, 1].Value = "هیچ رزروی در این بازه زمانی یافت نشد";
                worksheet.Cells[4, 1, 4, 7].Merge = true;
                worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
        }
        #endregion


        #region ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheCustomers
        public async Task<FileContentResult> ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheCustomers(DateTime fromDate, DateTime toDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var allReservesQuery = _NarijeDBContext.vReserves
                .Where(r => r.DateTime.Date >= fromDate.Date && r.DateTime.Date <= toDate.Date && r.Num > 0);

            var allReserves = await allReservesQuery.ToListAsync();

            if (!allReserves.Any())
                throw new Exception("در این بازه رزرو یافت نشد");

            var normalReserves = allReserves.Where(r => r.State != (int)EnumReserveState.perdict).ToList();
            var predictReserves = allReserves.Where(r => r.State == (int)EnumReserveState.perdict).ToList();

            var company = await _NarijeDBContext.Settings
                .Select(A => A.CompanyName)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var persianCalendar = new PersianCalendar();
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toDate)}/{persianCalendar.GetMonth(toDate):D2}/{persianCalendar.GetDayOfMonth(toDate):D2}";


            var shamsiDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
                .Select(offset => fromDate.AddDays(offset))
                .Select(date => (dynamic)new
                {
                    GregorianDate = date,
                    ShamsiDate = $"{persianCalendar.GetYear(date)}/{persianCalendar.GetMonth(date):D2}/{persianCalendar.GetDayOfMonth(date):D2}"
                })
                .ToList();
            var meals = await _NarijeDBContext.Meal.ToListAsync();
            var fileName = $"گزارش تفاوت بین پیش‌بینی و رزرو عادی بر اساس تاریخ {DateTime.Now:yyyy-MM-dd}.xlsx";
            using (var package = new ExcelPackage())
            {
                CreateCustomerDifferenceWorksheet(package, "همه", normalReserves, predictReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, company);
                foreach (var meal in meals)
                {
                    var mealNormalReserves = normalReserves.Where(r => r.MealType == meal.Id).ToList();
                    var mealPredictReserves = predictReserves.Where(r => r.MealType == meal.Id).ToList();
                    CreateCustomerDifferenceWorksheet(package, meal.Title, mealNormalReserves, mealPredictReserves, shamsiDates, shamsiPeriodStart, shamsiPeriodEnd, company);
                }

                var excelBytes = package.GetAsByteArray();
                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        private void CreateCustomerDifferenceWorksheet(ExcelPackage package, string sheetTitle, List<vReserve> normalReserves, List<vReserve> predictReserves, List<dynamic> shamsiDates, string shamsiPeriodStart, string shamsiPeriodEnd, string company)
        {
            var worksheet = package.Workbook.Worksheets.Add(sheetTitle);
            worksheet.View.RightToLeft = true;

            worksheet.Cells[1, 1].Value = "گزارش مغایرت سفارش مشتری با مقدار پیش بینی ( به تفکیک مشتری )";
            worksheet.Cells[1, 1, 1, 12].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 14;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells[2, 1].Value = $"بازه زمانی از {shamsiPeriodStart} لغایت {shamsiPeriodEnd}";
            worksheet.Cells[2, 1, 2, 12].Merge = true;
            worksheet.Cells[2, 1].Style.Font.Bold = true;
            worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            string[] headers = {
        "تاریخ",
        "کد شرکت",
        "نام شرکت",
        "کد شعبه",          // CustomerCode
        "نام شعبه",         // CustomerTitle
        "نام شعبه خدمات دهنده", // BranchTitle
        "کد کالا",
        "نام کالا",
        "مقدار پیش بینی",
        "مقدار سفارش مشتری",
        "اختلاف سفارش مشتری از مقدار پیش بینی",
        "درصد اختلاف"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
            }

            var groupedNormal = normalReserves
                .GroupBy(r => new
                {
                    Date = r.DateTime.Date,
                    CompanyCode = r.CustomerParentCode,
                    CompanyName = r.CustomerParentTitle,
                    BranchCode = r.CustomerCode,        // کد شعبه
                    BranchName = r.CustomerTitle,       // نام شعبه
                    ServiceBranchName = r.BranchTitle,  // نام شعبه خدمات دهنده
                    r.FoodArpaNumber,
                    r.FoodTitle
                })
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Num));

            var groupedPredict = predictReserves
                .GroupBy(r => new
                {
                    Date = r.DateTime.Date,
                    CompanyCode = r.CustomerParentCode,
                    CompanyName = r.CustomerParentTitle,
                    BranchCode = r.CustomerCode,        // کد شعبه
                    BranchName = r.CustomerTitle,       // نام شعبه
                    ServiceBranchName = r.BranchTitle,  // نام شعبه خدمات دهنده
                    r.FoodArpaNumber,
                    r.FoodTitle
                })
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Num));

            var allCustomerReserves = normalReserves.Select(r => new
            {
                r.DateTime.Date,
                CompanyCode = r.CustomerParentCode,
                CompanyName = r.CustomerParentTitle,
                BranchCode = r.CustomerCode,        // کد شعبه
                BranchName = r.CustomerTitle,       // نام شعبه
                ServiceBranchName = r.BranchTitle,  // نام شعبه خدمات دهنده
                r.FoodArpaNumber,
                r.FoodTitle
            })
                .Concat(predictReserves.Select(r => new
                {
                    r.DateTime.Date,
                    CompanyCode = r.CustomerParentCode,
                    CompanyName = r.CustomerParentTitle,
                    BranchCode = r.CustomerCode,       // کد شعبه
                    BranchName = r.CustomerTitle,       // نام شعبه
                    ServiceBranchName = r.BranchTitle,  // نام شعبه خدمات دهنده
                    r.FoodArpaNumber,
                    r.FoodTitle
                }))
                .Distinct()
                .OrderBy(r => r.Date)
                .ThenBy(r => r.CompanyName)
                .ThenBy(r => r.BranchName)
                .ThenBy(r => r.FoodTitle)
                .ToList();

            int currentRow = 4;
            foreach (var reserve in allCustomerReserves)
            {
                var dateInfo = shamsiDates.FirstOrDefault(d => d.GregorianDate.Date == reserve.Date);
                if (dateInfo == null) continue;

                var key = new
                {
                    reserve.Date,
                    reserve.CompanyCode,
                    reserve.CompanyName,
                    reserve.BranchCode,
                    reserve.BranchName,
                    reserve.ServiceBranchName,
                    reserve.FoodArpaNumber,
                    reserve.FoodTitle
                };

                bool hasNormal = groupedNormal.TryGetValue(key, out int normalValue);
                bool hasPredict = groupedPredict.TryGetValue(key, out int predictValue);

                if (!hasNormal && !hasPredict)
                {
                    continue;
                }

                normalValue = hasNormal ? normalValue : 0;
                predictValue = hasPredict ? predictValue : 0;

                var difference = normalValue - predictValue;
                var percentage = predictValue != 0 ? (double)difference / predictValue : 0;

                string formattedDifference = difference > 0 ? $"+{difference}" : difference.ToString();
                string formattedPercentage = predictValue > 0 ? percentage > 0 ? $"+{percentage:P2}" : percentage < 0 ? percentage.ToString("P2") : "0%" : "-100%";

                worksheet.Cells[currentRow, 1].Value = dateInfo.ShamsiDate;
                worksheet.Cells[currentRow, 2].Value = reserve.CompanyCode;
                worksheet.Cells[currentRow, 3].Value = reserve.CompanyName;
                worksheet.Cells[currentRow, 4].Value = reserve.BranchCode;      
                worksheet.Cells[currentRow, 5].Value = reserve.BranchName;        
                worksheet.Cells[currentRow, 6].Value = reserve.ServiceBranchName;  
                worksheet.Cells[currentRow, 7].Value = reserve.FoodArpaNumber;
                worksheet.Cells[currentRow, 8].Value = reserve.FoodTitle;
                worksheet.Cells[currentRow, 9].Value = predictValue;
                worksheet.Cells[currentRow, 10].Value = normalValue;
                worksheet.Cells[currentRow, 11].Value = formattedDifference;
                worksheet.Cells[currentRow, 12].Value = formattedPercentage;

                currentRow++;
            }

            if (currentRow > 4)
            {
                var allCells = worksheet.Cells[1, 1, currentRow - 1, 12];
                allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                allCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                for (int col = 1; col <= 12; col++)
                {
                    worksheet.Column(col).Width = 20;
                }
            }
            else
            {
                worksheet.Cells[4, 1].Value = "هیچ رزروی در این بازه زمانی یافت نشد";
                worksheet.Cells[4, 1, 4, 12].Merge = true;
                worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
        }
        #endregion
        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(ReserveInsertRequest request)
        {
            var Reserve = new Reserve()
            {
                UserId = request.userId,
                CustomerId = request.customerId,
                Num = request.num,
                FoodId = request.foodId,
                DateTime = request.dateTime,
                State = request.state,
                CreatedAt = DateTime.Now,
                ReserveType = request.reserveType,
                FoodType = request.foodType,
                Price = request.price,

            };


            await _NarijeDBContext.Reserves.AddAsync(Reserve);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Reserve.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(ReserveEditRequest request)
        {
            var Reserve = await _NarijeDBContext.Reserves
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (Reserve is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Reserve.UserId = request.userId;
            Reserve.CustomerId = request.customerId;
            Reserve.Num = request.num;
            Reserve.FoodId = request.foodId;
            Reserve.DateTime = request.dateTime;
            Reserve.State = request.state;
            Reserve.UpdatedAt = DateTime.Now;
            Reserve.ReserveType = request.reserveType;
            Reserve.FoodType = request.foodType;
            Reserve.Price = request.price;



            _NarijeDBContext.Reserves.Update(Reserve);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Reserve.Id);
        }
        #endregion
        
        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Reserve = await _NarijeDBContext.Reserves
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Reserve is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.Reserves.Remove(Reserve);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(ReserveCloneRequest request)
        {
            var Reserve = await _NarijeDBContext.Reserves
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Reserve is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion

        #region Export
        public async Task<ApiResponse> ExportAsync()
        {
            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Reserve", true);
            var query = FilterHelper.GetQuery(dbheader, _IHttpContextAccessor);

            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            List<FieldResponse> headers = new();

            result.header = dbheader.Select(A => A.title).ToList();

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;

            var Q = _NarijeDBContext.vReserves.Where(c => c.Num != 0)
                        .ProjectTo<ReserveResponse>(_IMapper.ConfigurationProvider);

            var Param = HttpUtility.ParseQueryString(_IHttpContextAccessor.HttpContext.Request.QueryString.ToString());
            var key = Param.AllKeys.Where(A => A.Equals("ids")).FirstOrDefault();
            if (key != null)
            {
                var ids = Param[key.ToString()];
                if (ids != "")
                {
                    int[] nids = ids.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    Q = Q.Where(A => nids.Contains(A.id));
                }
            }

            var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();
            switch (user.Role)
            {
                case (int)EnumRole.user:
                    Q = Q.Where(A => A.userId == user.Id);
                    break;
                case (int)EnumRole.customer:
                    Q = Q.Where(A => A.customerId == user.CustomerId);
                    break;
            }

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var data = Q.ToList<object>();

            result.body = ExportHelper.MakeResult(data, dbheader, false);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);

        }
        #endregion

        #region GetByFoodAsync
        // ------------------
        //  GetByFoodAsync
        // ------------------
        public async Task<ApiResponse> GetByFoodAsync()
        {
            try
            {

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "ReserveByFood");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                int i = 0;
                var Q = _NarijeDBContext.Reserves
                                         .Select(A => new ReportResponse()
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             state = A.State,
                                             userId = A.UserId,
                                             userName = A.User.Fname + " " + A.User.Lname,
                                             //   userDescription = A.User.Description,
                                             customer = A.Customer.Title,
                                             customerId = A.CustomerId,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             foodGroupId = A.Food.GroupId,
                                             foodGroup = A.Food.Group.Title,
                                             isFood = A.Food.IsFood,
                                             foodType = A.FoodType,
                                             qty = A.Num
                                         });

                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                var reserves = await Q.ToListAsync();

                var foodreserves = reserves.Select(A => A.foodId).Distinct().ToList();
                var customerreserves = reserves.Select(A => A.customerId).Distinct().ToList();

                var foods = await _NarijeDBContext.Foods.Where(A => foodreserves.Contains(A.Id)).ToListAsync();
                var customers = await _NarijeDBContext.Customers.Where(A => customerreserves.Contains(A.Id)).ToListAsync();

                List<List<string>> body = new();
                foreach (var customer in customers)
                {
                    List<string> items = new();
                    items.Add(customer.Title);
                    foreach (var food in foods)
                    {
                        i = reserves.Where(A => A.foodId == food.Id && A.customerId == customer.Id).Sum(A => A.qty);
                        items.Add(i.ToString());
                    }
                    body.Add(items);
                    i = reserves.Where(A => A.customerId == customer.Id).Sum(A => A.qty);
                    items.Add(i.ToString());
                }

                /*
                List<string> sum = new();
                foreach (var food in foods)
                {
                    i = reserves.Where(A => A.foodId == food.Id).Sum(A => A.qty);
                    sum.Add(i.ToString());
                }
                i = reserves.Sum(A => A.qty);
                sum.Add(i.ToString());
                */

                return new ApiOkResponse(_Message: "SUCCESS", _Data: body, _Meta: null, _Header: header);

            }
            catch (Exception Ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "خطایی رخ داده است" + Ex.Message);
            }
        }
        #endregion


        #region GetAllByParamsAsync
        // ------------------
        //  GetAllByParamsAsync
        // ------------------
        public async Task<ApiResponse> GetAllByParamsAsync(int? page, int? limit, int paramsId, string ParamsName, string headerName)
        {
            // Set default pagination values
            page ??= 1;
            limit ??= 30;

            // Check user authorization (common logic)
            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Reserve");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);
            var identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;

            if (identity == null || identity.Claims.Count() == 0)
                return new ApiErrorResponse(StatusCodes.Status403Forbidden, "کاربر دسترسی ندارد");

            var user = await _NarijeDBContext.Users
                .Where(u => u.Id == int.Parse(identity.FindFirst("Id").Value))
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (user == null)
                return new ApiErrorResponse(StatusCodes.Status403Forbidden, "کابر یافت نشد");


            IQueryable<vReserve> reservesQuery = _NarijeDBContext.vReserves.AsNoTracking();


            reservesQuery = ParamsName.ToLower() switch
            {
                "foodid" => reservesQuery.Where(r => r.FoodId == paramsId && r.Num > 0 && r.State != (int)EnumReserveState.perdict),
                "userid" => reservesQuery.Where(r => r.UserId == paramsId && r.Num > 0 && r.State != (int)EnumReserveState.perdict),
                "customerid" => reservesQuery.Where(r => r.CustomerId == paramsId && r.Num > 0 && r.State != (int)EnumReserveState.perdict),
                _ => throw new ArgumentException("Invalid ParamsName provided")
            };


            var projectedQuery = reservesQuery.ProjectTo<ReserveResponse>(_IMapper.ConfigurationProvider);
            projectedQuery = user.Role switch
            {
                (int)EnumRole.user => projectedQuery.Where(r => r.userId == user.Id),
                (int)EnumRole.customer => projectedQuery.Where(r => r.customerId == user.CustomerId),
                _ => projectedQuery
            };


            if (!string.IsNullOrEmpty(query.Search) || query.Filter.Count > 0 || query.Sort.Count > 0)
                projectedQuery = projectedQuery.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);


            var summary = ParamsName.ToLower() switch
            {
                "foodid" => await reservesQuery
                    .GroupBy(r => r.FoodId)
                    .Select(g => new
                    {
                        FirstReserveDate = g.Min(r => r.CreatedAt),
                        LastReserveDate = g.Max(r => r.CreatedAt),
                        ReserveCount = g.Count(),
                        TotalPrice = g.Sum(r => r.Price)

                    })
                    .FirstOrDefaultAsync(),

                "userid" => await reservesQuery
                    .GroupBy(r => r.UserId)
                    .Select(g => new
                    {
                        FirstReserveDate = g.Min(r => r.CreatedAt),
                        LastReserveDate = g.Max(r => r.CreatedAt),
                        ReserveCount = g.Count(),
                        TotalPrice = g.Sum(r => r.Price)
                    })
                    .FirstOrDefaultAsync(),

                "customerid" => await reservesQuery
                    .GroupBy(r => r.CustomerId)
                    .Select(g => new
                    {
                        FirstReserveDate = g.Min(r => r.CreatedAt),
                        LastReserveDate = g.Max(r => r.CreatedAt),
                        ReserveCount = g.Count(),
                        TotalPrice = g.Sum(r => r.Price)
                    })
                    .FirstOrDefaultAsync(),

                _ => throw new ArgumentException("Invalid ParamsName provided")
            };


            var paginatedReserves = await projectedQuery.GetPaged(page.Value, limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: paginatedReserves.Data, _Meta: paginatedReserves.Meta, _Header: header, _ExtraObject: summary);
        }

        #endregion


   

        public class AccessoryData
        {
            public string FoodTitle { get; set; }
            public string ArpaNumber { get; set; }
            public int Quantity { get; set; }
        }
    }
}


