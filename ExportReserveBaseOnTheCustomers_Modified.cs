using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

public async Task<FileContentResult> ExportReserveBaseOnTheCustomers(DateTime fromDate, DateTime toDate, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false)
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
                            .Select(A => A.CompanyName)
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

    // Get all active customers even if they don't have reservations
    var allActiveCustomers = await _NarijeDBContext.Customer
        .Where(c => c.IsActive == true)
        .Select(c => new {
            c.Id,
            c.Title,
            c.Code,
            c.ParentId,
            c.BranchId,
            ParentTitle = _NarijeDBContext.Customer.Where(p => p.Id == c.ParentId).Select(p => p.Title).FirstOrDefault(),
            ParentCode = _NarijeDBContext.Customer.Where(p => p.Id == c.ParentId).Select(p => p.Code).FirstOrDefault(),
            BranchTitle = _NarijeDBContext.Branch.Where(b => b.Id == c.BranchId).Select(b => b.Title).FirstOrDefault()
        })
        .ToListAsync();

    // Get default branch if exists
    var defaultBranch = reserves.FirstOrDefault(r => r.BranchId != null && r.BranchId != 0);

    // Add customers without reservations to the reserves list with zero quantities
    var customersWithReserves = reserves.Select(r => r.CustomerId).Distinct().ToList();
    var customersWithoutReserves = allActiveCustomers.Where(c => !customersWithReserves.Contains(c.Id)).ToList();

    // Create dummy reserve entries for customers without reservations
    foreach (var customer in customersWithoutReserves)
    {
        reserves.Add(new vReserve
        {
            CustomerId = customer.Id,
            CustomerTitle = customer.Title,
            CustomerCode = customer.Code,
            CustomerParentId = customer.ParentId,
            CustomerParentTitle = customer.ParentTitle,
            CustomerParentCode = customer.ParentCode,
            Num = 0,
            FoodTitle = "",
            DateTime = fromDate,
            BranchId = customer.BranchId ?? defaultBranch?.BranchId ?? 0,
            BranchTitle = customer.BranchTitle ?? defaultBranch?.BranchTitle ?? "",
            DeliverHour = "",
            IsFood = false,
            MealType = 0
        });
    }

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
    var shamsiDate = $"{persianCalendar.GetYear(fromDate)}/{persianCalendar.GetMonth(fromDate):D2}/{persianCalendar.GetDayOfMonth(fromDate):D2}";
    var dayOfWeek = persianCalendar.GetDayOfWeek(fromDate);
    var persianDayName = GetPersianDayName(dayOfWeek);
    var meals = await _NarijeDBContext.Meal.ToListAsync();

    var dateWithDayName = $"{shamsiDate} {persianDayName}";
    var fileName = $"گزارش تفکیکی بر اساس مشتریان {DateTime.Now:yyyy-MM-dd}.xlsx";
    using (var package = new ExcelPackage())
    {
        // Change "همه" to "لیست غذایی"
        CreateMealWorksheetForCustomers(package, "لیست غذایی", reserves, dateWithDayName, "", showAccessory, accessoryCompanies, company);
        foreach (var meal in meals)
        {
            var mealReserves = reserves.Where(r => r.MealType == meal.Id).ToList();
            if (mealReserves.Any(r => r.Num > 0))
            {
                CreateMealWorksheetForCustomers(package, meal.Title, mealReserves, dateWithDayName, "", showAccessory, accessoryCompanies, company);
            }
        }

        var excelBytes = package.GetAsByteArray();
        return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = fileName
        };
    }
}

private void CreateMealWorksheetForCustomers(ExcelPackage package, string mealTitle, List<vReserve> reserves, string dateWithDayName, string shamsiPeriodEnd, bool showAccessory, List<AccessoryCompany> accessoryCompanies, string company)
{
    var allFoods = reserves.Where(r => !string.IsNullOrEmpty(r.FoodTitle))
        .Select(r => new { Title = r.FoodTitle, Arpa = r.FoodArpaNumber, Category = r.Category, IsFood = r.IsFood })
        .Distinct()
        .OrderBy(f => f.Title)
        .ToList();
    
    var branchColors = new List<Color>
    {
        Color.FromArgb(198, 239, 206), // Light green
        Color.FromArgb(198, 224, 180), // Another green shade
        Color.FromArgb(255, 235, 156), // Light yellow
        Color.FromArgb(255, 242, 204), // Light cream
        Color.FromArgb(220, 230, 241)  // Light blue
    };

    if (showAccessory && accessoryCompanies != null)
    {
        var accessories = accessoryCompanies.Select(ac => new { 
            Title = ac.Accessory.Title, 
            Arpa = ac.Accessory.ArpaNumber, 
            Category = "اکسسوری", 
            IsFood = false 
        }).Distinct().ToList();
        allFoods = allFoods.Concat(accessories).OrderBy(f => f.Title).ToList();
    }

    var worksheet = package.Workbook.Worksheets.Add(mealTitle);
    var foodCount = allFoods.Count();
    // 5 base columns (removed company code) + 1 for جمع column + food columns
    int totalColumns = foodCount + 6;
    var branchColorMap = new Dictionary<int, Color>();

    // Set worksheet properties
    worksheet.View.RightToLeft = true;
    worksheet.Row(1).Height = 60;

    // Set page setup for A4 landscape
    worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
    worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
    worksheet.PrinterSettings.FitToPage = true;
    worksheet.PrinterSettings.FitToWidth = 1;
    worksheet.PrinterSettings.FitToHeight = 0;
    worksheet.PrinterSettings.Margins.Top = 0.25m;
    worksheet.PrinterSettings.Margins.Bottom = 0.25m;
    worksheet.PrinterSettings.Margins.Left = 0.25m;
    worksheet.PrinterSettings.Margins.Right = 0.25m;

    // Header row 1
    worksheet.Cells[1, 1].Value = company + " , " + "فرم گزارش سفارش روزانه مشتریان";
    worksheet.Cells[1, 1, 1, 5].Merge = true;
    worksheet.Cells[1, 1].Style.Font.Bold = true;
    worksheet.Cells[1, 1].Style.Font.Size = 12;
    worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
    worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

    var currentDate = DateTime.Now;
    var shamsiCurrentDate = $"{new PersianCalendar().GetYear(currentDate)}/{new PersianCalendar().GetMonth(currentDate):D2}/{new PersianCalendar().GetDayOfMonth(currentDate):D2}";
    var currentTime = $"{currentDate:HH:mm}"; // Removed seconds
    var dateRange = worksheet.Cells[1, 6, 1, totalColumns];
    dateRange.Merge = true;
    dateRange.Value = $"تاریخ گزارش‌گیری: {shamsiCurrentDate}\nساعت گزارش‌گیری: {currentTime}";
    dateRange.Style.Font.Bold = true;
    dateRange.Style.Font.Size = 10;
    dateRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
    dateRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    dateRange.Style.WrapText = true;

    worksheet.Cells[1, 1, 1, totalColumns].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

    // Row 2 - Meal title and food names
    worksheet.Cells[2, 1, 2, 5].Merge = true;
    worksheet.Cells[2, 1].Value = $"وعده غذایی: {mealTitle}";
    worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[2, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    worksheet.Cells[2, 1].Style.Font.Bold = true;
    worksheet.Row(2).Height = 100;

    var branches = reserves.Where(r => r.BranchId != null && r.BranchId != 0)
        .Select(r => new { BranchId = r.BranchId ?? 0, BranchTitle = r.BranchTitle })
        .Distinct()
        .OrderBy(b => b.BranchTitle)
        .ToList();

    // Map branch colors
    for (int i = 0; i < branches.Count; i++)
    {
        branchColorMap[branches[i].BranchId] = branchColors[i % branchColors.Count];
    }

    int colIndex = 6;
    worksheet.Cells[2, colIndex].Value = "نام کالا";
    worksheet.Cells[2, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[2, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    worksheet.Cells[2, colIndex].Style.Font.Bold = true;
    colIndex++;
    
    // Add food names rotated 90 degrees with colors for food items only
    foreach (var food in allFoods)
    {
        worksheet.Cells[2, colIndex].Value = food.Title;
        worksheet.Cells[2, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[2, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
        worksheet.Cells[2, colIndex].Style.TextRotation = 90;
        worksheet.Cells[2, colIndex].Style.Font.Bold = true;
        worksheet.Cells[2, colIndex].Style.Font.Size = 9;
        
        // Apply yellow background only to food columns (not accessories)
        if (food.IsFood)
        {
            worksheet.Cells[2, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[2, colIndex].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 204));
        }
        colIndex++;
    }

    // Row 3 - Product codes
    colIndex = 6;
    worksheet.Cells[3, colIndex].Value = "کد کالا";
    worksheet.Cells[3, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[3, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    worksheet.Cells[3, colIndex].Style.Font.Bold = true;
    colIndex++;
    
    // Add product codes rotated 90 degrees
    foreach (var food in allFoods)
    {
        worksheet.Cells[3, colIndex].Value = food.Arpa ?? "";
        worksheet.Cells[3, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[3, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
        worksheet.Cells[3, colIndex].Style.TextRotation = 90;
        worksheet.Cells[3, colIndex].Style.Font.Bold = true;
        worksheet.Cells[3, colIndex].Style.Font.Size = 8;
        
        // Apply yellow background only to food columns
        if (food.IsFood)
        {
            worksheet.Cells[3, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[3, colIndex].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 204));
        }
        colIndex++;
    }

    // Date cell - merge for all branch rows
    int branchSectionRows = branches.Count * 2; // Each branch has 2 rows (branch total + food total)
    worksheet.Cells[3, 1, 4 + branchSectionRows, 5].Merge = true;
    worksheet.Cells[3, 1].Value = $"تاریخ ارایه غذا:\n{dateWithDayName}";
    worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[3, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    worksheet.Cells[3, 1].Style.Font.Bold = true;
    worksheet.Cells[3, 1].Style.WrapText = true;

    // Row 4 - Total sum row
    colIndex = 6;
    worksheet.Cells[4, colIndex].Value = "جمع کل";
    worksheet.Cells[4, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    worksheet.Cells[4, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    worksheet.Cells[4, colIndex].Style.Font.Bold = true;
    colIndex++;
    
    foreach (var food in allFoods)
    {
        var total = reserves.Where(r => r.FoodTitle == food.Title).Sum(r => r.Num);
        if (showAccessory && food.Category == "اکسسوری")
        {
            var accessoryTotal = accessoryCompanies?.Where(ac => ac.Accessory.Title == food.Title).Sum(ac => ac.Numbers) ?? 0;
            total += accessoryTotal;
        }
        
        if (total == 0)
        {
            worksheet.Cells[4, colIndex].Value = "";
            worksheet.Cells[4, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[4, colIndex].Style.Fill.BackgroundColor.SetColor(Color.White);
        }
        else
        {
            worksheet.Cells[4, colIndex].Value = total;
            worksheet.Cells[4, colIndex].Style.Font.Bold = true;
            
            // Apply yellow background only to food columns with values
            if (food.IsFood)
            {
                worksheet.Cells[4, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, colIndex].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 204));
            }
        }
        
        worksheet.Cells[4, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[4, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        colIndex++;
    }

    // Branch rows
    int rowIndex = 5;
    foreach (var branch in branches)
    {
        var branchColor = branchColorMap[branch.BranchId];
        
        // Branch total row
        worksheet.Cells[rowIndex, 6].Value = branch.BranchTitle;
        worksheet.Cells[rowIndex, 6].Style.Font.Bold = true;
        worksheet.Cells[rowIndex, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[rowIndex, 6].Style.Fill.BackgroundColor.SetColor(branchColor);
        
        colIndex = 7;
        int branchMainFoodTotal = 0;
        foreach (var food in allFoods)
        {
            var branchTotal = reserves.Where(r => r.BranchId == branch.BranchId && r.FoodTitle == food.Title).Sum(r => r.Num);
            if (showAccessory && food.Category == "اکسسوری")
            {
                var branchCustomerIds = reserves
                    .Where(r => r.BranchId == branch.BranchId)
                    .Select(r => r.CustomerId)
                    .Distinct()
                    .ToList();

                var accessoryTotal = accessoryCompanies
                    ?.Where(ac => branchCustomerIds.Contains(ac.CompanyId) && ac.Accessory.Title == food.Title)
                    .Sum(ac => ac.Numbers) ?? 0;

                branchTotal += accessoryTotal;
            }
            
            if (food.IsFood)
            {
                branchMainFoodTotal += branchTotal;
            }
            
            if (branchTotal == 0)
            {
                worksheet.Cells[rowIndex, colIndex].Value = "";
                worksheet.Cells[rowIndex, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(Color.White);
            }
            else
            {
                worksheet.Cells[rowIndex, colIndex].Value = branchTotal;
                worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;
                
                // Apply branch color only to food columns with values
                if (food.IsFood)
                {
                    worksheet.Cells[rowIndex, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(branchColor);
                }
            }
            
            worksheet.Cells[rowIndex, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[rowIndex, colIndex].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            colIndex++;
        }
        
        rowIndex++;
        
        // Branch main food total row
        worksheet.Cells[rowIndex, 6].Value = $"جمع غذای اصلی {branch.BranchTitle}";
        worksheet.Cells[rowIndex, 6].Style.Font.Bold = true;
        worksheet.Cells[rowIndex, 6].Style.Font.Size = 9;
        worksheet.Cells[rowIndex, 7].Value = branchMainFoodTotal;
        worksheet.Cells[rowIndex, 7].Style.Font.Bold = true;
        worksheet.Cells[rowIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells[rowIndex, 6, rowIndex, totalColumns].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[rowIndex, 6, rowIndex, totalColumns].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
        
        rowIndex++;
    }

    //-------------------------------- Customer data rows ------------------------------------------//
    rowIndex++; // Add space before customer headers
    
    // Customer headers
    worksheet.Cells[rowIndex, 1].Value = "شعبه خدمات دهنده";
    worksheet.Cells[rowIndex, 2].Value = "نام مشتری";
    worksheet.Cells[rowIndex, 3].Value = "کد شعبه";
    worksheet.Cells[rowIndex, 4].Value = "نام شعبه";
    worksheet.Cells[rowIndex, 5].Value = "ساعت تحویل";
    worksheet.Cells[rowIndex, 6].Value = "جمع غذا";
    
    // Make headers bold
    for (int i = 1; i <= 6; i++)
    {
        worksheet.Cells[rowIndex, i].Style.Font.Bold = true;
        worksheet.Cells[rowIndex, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[rowIndex, i].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
    }
    rowIndex++;

    // Sort customers: First group by parent company, then by customer title
    var customerGroups = reserves
        .Where(r => r.CustomerId > 0)
        .GroupBy(r => new { 
            r.CustomerId,
            // Combine parent and child names
            CustomerDisplayName = string.IsNullOrEmpty(r.CustomerParentTitle) 
                ? r.CustomerTitle 
                : $"{r.CustomerParentTitle} - {r.CustomerTitle}",
            r.CustomerTitle,
            r.CustomerCode,
            r.CustomerParentId,
            r.CustomerParentCode,
            r.CustomerParentTitle
        })
        .OrderBy(g => g.Key.CustomerParentId ?? int.MaxValue) // Customers with parents first
        .ThenBy(g => g.Key.CustomerParentTitle ?? "")
        .ThenBy(g => g.Key.CustomerTitle)
        .ToList();

    // Group customers by branch for proper display
    var customersByBranch = new Dictionary<int, List<dynamic>>();
    foreach (var customerGroup in customerGroups)
    {
        var customerReserves = customerGroup.ToList();
        var branchId = customerReserves.FirstOrDefault()?.BranchId ?? 0;
        
        if (!customersByBranch.ContainsKey(branchId))
        {
            customersByBranch[branchId] = new List<dynamic>();
        }
        
        customersByBranch[branchId].Add(new {
            Customer = customerGroup.Key,
            Reserves = customerReserves
        });
    }

    // Display customers grouped by branch
    foreach (var branch in branches)
    {
        if (!customersByBranch.ContainsKey(branch.BranchId))
            continue;
            
        int branchStartRow = rowIndex;
        var branchCustomers = customersByBranch[branch.BranchId];
        var branchColor = branchColorMap[branch.BranchId];

        foreach (dynamic customerData in branchCustomers)
        {
            var customer = customerData.Customer;
            List<vReserve> customerReserves = customerData.Reserves;
            
            // Branch name (will be merged later)
            worksheet.Cells[rowIndex, 1].Value = branch.BranchTitle;
            worksheet.Cells[rowIndex, 1].Style.Font.Bold = true;
            
            // Combined customer name (parent - child)
            worksheet.Cells[rowIndex, 2].Value = customer.CustomerDisplayName;
            worksheet.Cells[rowIndex, 2].Style.Font.Bold = true;
            
            // Customer code
            worksheet.Cells[rowIndex, 3].Value = customer.CustomerCode;
            worksheet.Cells[rowIndex, 3].Style.Font.Bold = true;
            
            // Branch name for customer
            worksheet.Cells[rowIndex, 4].Value = customer.CustomerTitle;
            worksheet.Cells[rowIndex, 4].Style.Font.Bold = true;
            
            // Format delivery time without seconds
            var deliverHour = customerReserves.FirstOrDefault(r => !string.IsNullOrEmpty(r.DeliverHour))?.DeliverHour ?? "";
            if (!string.IsNullOrEmpty(deliverHour) && deliverHour.Contains(":"))
            {
                var timeParts = deliverHour.Split(':');
                if (timeParts.Length >= 2)
                {
                    deliverHour = $"{timeParts[0]}:{timeParts[1]}";
                }
            }
            worksheet.Cells[rowIndex, 5].Value = deliverHour;
            worksheet.Cells[rowIndex, 5].Style.Font.Bold = true;

            // Calculate main food total for this customer (only IsFood = true)
            int customerMainFoodTotal = customerReserves.Where(r => r.IsFood == true).Sum(r => r.Num);
            worksheet.Cells[rowIndex, 6].Value = customerMainFoodTotal > 0 ? customerMainFoodTotal.ToString() : "0";
            worksheet.Cells[rowIndex, 6].Style.Font.Bold = true;
            worksheet.Cells[rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Food columns
            colIndex = 7;
            foreach (var food in allFoods)
            {
                var foodTotal = customerReserves
                    .Where(r => r.FoodTitle == food.Title)
                    .Sum(r => r.Num);
                    
                if (showAccessory && food.Category == "اکسسوری")
                {
                    var accessoryTotal = accessoryCompanies
                        ?.Where(ac => ac.CompanyId == customer.CustomerId && ac.Accessory.Title == food.Title)
                        .Sum(ac => ac.Numbers) ?? 0;
                    foodTotal += accessoryTotal;
                }
                
                if (foodTotal == 0)
                {
                    // Hide zeros - white background
                    worksheet.Cells[rowIndex, colIndex].Value = "";
                    worksheet.Cells[rowIndex, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(Color.White);
                }
                else
                {
                    worksheet.Cells[rowIndex, colIndex].Value = foodTotal;
                    worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;
                    
                    // Apply yellow background only to food columns with values
                    if (food.IsFood)
                    {
                        worksheet.Cells[rowIndex, colIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 204));
                    }
                }
                
                worksheet.Cells[rowIndex, colIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                colIndex++;
            }

            rowIndex++;
        }

        // Merge branch cells vertically
        if (branchStartRow < rowIndex && rowIndex - branchStartRow > 1)
        {
            worksheet.Cells[branchStartRow, 1, rowIndex - 1, 1].Merge = true;
            worksheet.Cells[branchStartRow, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }
    }

    // Apply borders to all cells
    var allCells = worksheet.Cells[1, 1, rowIndex - 1, totalColumns];
    allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
    allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
    allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
    allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    allCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

    // Set column widths - make food columns narrower
    worksheet.Column(1).Width = 15; // Branch column
    worksheet.Column(2).Width = 20; // Customer name column
    worksheet.Column(3).Width = 10; // Customer code column
    worksheet.Column(4).Width = 15; // Branch name column
    worksheet.Column(5).Width = 10; // Delivery time column
    worksheet.Column(6).Width = 10; // Total food column
    
    // Food columns - much narrower
    for (int i = 7; i <= totalColumns; i++)
    {
        worksheet.Column(i).Width = 6;
    }
    
    // Auto-fit rows with rotated text
    for (int i = 1; i <= rowIndex; i++)
    {
        if (i == 2 || i == 3)
        {
            worksheet.Row(i).Height = 100; // For rotated text rows
        }
        else
        {
            worksheet.Row(i).Height = 20; // Standard row height
        }
    }
}