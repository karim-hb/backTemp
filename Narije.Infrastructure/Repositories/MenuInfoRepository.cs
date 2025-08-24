using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Menu;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Narije.Core.DTOs.ViewModels.MenuInfo;
using Narije.Core.DTOs.Enum;
using System.Drawing;
using OfficeOpenXml;
using System.Globalization;
using System.IO;
using DNTPersianUtils.Core;
using System.Security.Claims;

namespace Narije.Infrastructure.Repositories
{
    public class MenuInfoRepository : BaseRepository<MenuInfo>, IMenuInfoRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public MenuInfoRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }
        private async Task<Core.Entities.User> CheckAccess()
        {
            //Check Access
            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;
            int id = Convert.ToInt32(Identity.FindFirst("Id").Value);
            var User = await _NarijeDBContext.Users
                                     .Where(A => A.Id == id)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
            if (User is null)
                return null;

            if (User.Active == false)
                return null;

            return User;

        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            try
            {
                // Fetch MenuInfo data
                var menuInfo = await _NarijeDBContext.MenuInfo
                    .Where(A => A.Id == id)
                    .ProjectTo<MenuInfoResponse>(_IMapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                // Fetch menus with food and group details
                var menus = await _NarijeDBContext.Menus
                    .Where(A => A.MenuInfoId == id)
                    .ProjectTo<MenuResponse>(_IMapper.ConfigurationProvider)
                    .ToListAsync();





                // Return the API response
                return new ApiOkResponse(_Message: "SUCCEED", _Data: new { menuInfo, menus });
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status405MethodNotAllowed,
                    _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message
                );
            }
        }


        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit)
        {
            try
            {
                if ((page is null) || (page == 0))
                    page = 1;
                if ((limit is null) || (limit == 0))
                    limit = 30;

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "MenuInfo");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.MenuInfo
                    .Include(m => m.LastUpdaterUser)
                    .AsQueryable();

                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                var Menus = await Q.GetPaged(Page: page.Value, Limit: limit.Value);
                var ids = Menus.Data.Select(A => A.Id).ToList();
                var userIds = Menus.Data.Select(menu => menu.LastUpdaterUserId).Distinct().ToList();

                var menuinfos = await _NarijeDBContext.CustomerMenuInfo
                    .Include(cmi => cmi.Customer)
                    .Where(cmi => ids.Contains(cmi.MenuInfoId))
                    .AsNoTracking()
                    .ToListAsync();

                var users = await _NarijeDBContext.Users
               .Where(u => userIds.Contains(u.Id))
               .AsNoTracking()
               .ToDictionaryAsync(u => u.Id);

                var result = Menus.Data.Select(menu => new MenuInfoResponse
                {
                    id = menu.Id,
                    title = menu.Title,
                    description = menu.Description,
                    month = menu.Month,
                    year = menu.Year,
                    active = menu.Active,
                    lastUpdaterUserId = menu.LastUpdaterUserId,
                    lastUpdaterUser = menu.LastUpdaterUserId.HasValue ? users.TryGetValue(menu.LastUpdaterUserId ?? 0, out var user)
                ? $"{user.Fname} {user.Lname}"
                : null : null,
                    customers = string.Join(", ", menuinfos
                        .Where(cmi => cmi.MenuInfoId == menu.Id)
                        .Select(cmi => cmi.Customer.Title)),
                    updatedAt = menu.UpdatedAt
                }).ToList();

                return new ApiOkResponse(_Message: "SUCCESS", _Data: result, _Meta: Menus.Meta, _Header: header);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(MenuInfoInserRequest request)
        {
            using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();
            try
            {
                var menuInfo = new MenuInfo()
                {
                    Title = request.title,
                    Description = request.description,
                    Month = request.month,
                    Year = request.year,
                    Active = request.active,
                };

                await _NarijeDBContext.MenuInfo.AddAsync(menuInfo);
                await _NarijeDBContext.SaveChangesAsync();

                if (request.Menu.Count > 0)
                {
                    var menus = request.Menu.Select(menu => new Menu()
                    {
                        DateTime = menu.dateTime,
                        FoodId = menu.foodId,
                        MaxReserve = menu.maxReserve,
                        FoodType = menu.foodType,
                        MealType = menu.mealType,
                        EchoPrice = menu.echoPrice,
                        SpecialPrice = menu.specialPrice,
                        MenuInfoId = menuInfo.Id,
                    }).ToList();

                    await _NarijeDBContext.Menus.AddRangeAsync(menus);
                    var result = await _NarijeDBContext.SaveChangesAsync();

                    if (result < menus.Count)
                    {
                        throw new Exception("Not all menus were inserted.");
                    }
                }

                await transaction.CommitAsync();
                return await GetAsync(menuInfo.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(MenuInfoEditRequest request)
        {
            using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();
            try
            {
                var MenuInfo = await _NarijeDBContext.MenuInfo
                                                      .Where(A => A.Id == request.id)
                                                      .FirstOrDefaultAsync();
                if (MenuInfo is null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

                MenuInfo.Title = request.title;
                MenuInfo.Description = request.description;
                MenuInfo.Month = request.month;
                MenuInfo.Year = request.year;
                MenuInfo.Active = request.active;

                MenuInfo.UpdatedAt = DateTime.Now;
                var user = await CheckAccess();
                MenuInfo.LastUpdaterUserId = user.Id;

                var existingMenus = await _NarijeDBContext.Menus
                .Where(m => m.MenuInfoId == request.id)
                .ToListAsync();

                var newMenuIds = request.Menu.Select(m => m.id).Where(id => id > 0).ToList();

                var menusToRemove = existingMenus.Where(em => !newMenuIds.Contains(em.Id)).ToList();

                foreach (var menu in request.Menu)
                {
                    if (menu.id == 0)
                    {
                        var newMenu = new Menu
                        {
                            DateTime = menu.dateTime,
                            FoodId = menu.foodId,
                            MaxReserve = menu.maxReserve,
                            FoodType = menu.foodType,
                            MealType = menu.mealType,
                            EchoPrice = menu.echoPrice,
                            SpecialPrice = menu.specialPrice,
                            MenuInfoId = MenuInfo.Id,
                        };
                        await _NarijeDBContext.Menus.AddAsync(newMenu);
                    }
                    else
                    {
                        var existingMenu = await _NarijeDBContext.Menus.FirstOrDefaultAsync(m => m.Id == menu.id);

                        if (existingMenu != null)
                        {
                            if (menu.maxReserve == 0)
                            {
                                var existReserves = await _NarijeDBContext.Reserves.AsNoTracking().Where(r => r.MenuId == menu.id).ToListAsync();
                                if (existReserves.Any())
                                {
                                    await transaction.RollbackAsync();
                                    return new ApiErrorResponse(
                                        _Code: StatusCodes.Status400BadRequest,
                                        _Message: "نمی‌توانید مقدار رزرو را به صفر تغییر دهید زیرا این منو دارای رزروهای فعال است."
                                    );
                                }
                            }

                            if (existingMenu.EchoPrice != menu.echoPrice)
                            {
                                var reserveIds = await _NarijeDBContext.Reserves.AsNoTracking()
                                .Include(r => r.Food)
                                  .Where(r =>
                                      r.MenuId == menu.id &&
                                      r.FoodType == (int)EnumFoodType.echo &&
                                      r.PriceType != null &&
                                        (

                                          r.PriceType == (int)EnumPrice.fromMenu ||


                                                (
                                                       r.PriceType == (int)EnumPrice.justVipFromMenu &&
                                                       r.Food != null &&
                                                       r.Food.Vip == true
                                                 )
                                           )
                                      )
                                       .Select(r => r.Id)
                                       .ToListAsync();

                                await UpdateReservePricesRawAsync(reserveIds, menu.echoPrice ?? 0);


                                await UpdateReservesAndWallet(
                                    existingMenu.Id,
                                    existingMenu.EchoPrice ?? 0,
                                    menu.echoPrice ?? 0,
                                    EnumFoodType.echo
                                );

                            }

                            if (existingMenu.SpecialPrice != menu.specialPrice)
                            {
                                var reserveIds = await _NarijeDBContext.Reserves.AsNoTracking()
                           .Include(r => r.Food)
                             .Where(r =>
                                 r.MenuId == menu.id &&
                                 r.FoodType == (int)EnumFoodType.special &&
                                 r.PriceType != null &&
                                   (

                                     r.PriceType == (int)EnumPrice.fromMenu ||


                                           (
                                                  r.PriceType == (int)EnumPrice.justVipFromMenu &&
                                                  r.Food != null &&
                                                  r.Food.Vip == true
                                            )
                                      )
                                 )
                                  .Select(r => r.Id)
                                  .ToListAsync();

                                await UpdateReservePricesRawAsync(reserveIds, menu.specialPrice ?? 0);
                                await UpdateReservesAndWallet(
                                    existingMenu.Id,
                                    existingMenu.SpecialPrice ?? 0,
                                    menu.specialPrice ?? 0,
                                    EnumFoodType.special
                                );

                            }
                            if (existingMenu.SpecialPrice != menu.specialPrice || existingMenu.EchoPrice != menu.echoPrice)
                            {
                                var menuLog = new MenuLog
                                {
                                    UserId = user.Id,
                                    FoodId = menu.foodId,
                                    MenuId = menu.id,
                                    MenuInfoId = request.id,
                                    EchoPriceBefore = existingMenu.EchoPrice,
                                    EchoPriceAfter = menu.echoPrice,
                                    SpecialPriceBefore = existingMenu.SpecialPrice,
                                    SpecialPriceAfter = menu.specialPrice,
                                    DateTime = DateTime.Now,
                                    MenuDateTime = menu.dateTime,

                                };
                                await _NarijeDBContext.MenuLogs.AddAsync(menuLog);
                            }


                            existingMenu.MaxReserve = menu.maxReserve;
                            existingMenu.EchoPrice = menu.echoPrice;
                            existingMenu.SpecialPrice = menu.specialPrice;

                            _NarijeDBContext.Menus.Update(existingMenu);
                        }
                        else
                        {
                            var newMenu = new Menu
                            {
                                DateTime = menu.dateTime,
                                FoodId = menu.foodId,
                                MaxReserve = menu.maxReserve,
                                FoodType = menu.foodType,
                                MealType = menu.mealType,
                                EchoPrice = menu.echoPrice,
                                SpecialPrice = menu.specialPrice,
                                MenuInfoId = MenuInfo.Id,
                            };
                            await _NarijeDBContext.Menus.AddAsync(newMenu);
                        }
                    }

                }

                foreach (var menuToRemove in menusToRemove)
                {
                    var reservesToRemove = await _NarijeDBContext.Reserves
                        .Where(r => r.MenuId == menuToRemove.Id)
                        .ToListAsync();

                    if (reservesToRemove.Any())
                    {
                        foreach (var reserve in reservesToRemove)
                        {
                            if (reserve.PayType == (int)EnumInvoicePayType.debit)
                            {
                                await DeleteReserve(menuToRemove, reserve);
                            }

                        }
                        _NarijeDBContext.Reserves.RemoveRange(reservesToRemove);

                    }

                    _NarijeDBContext.Menus.Remove(menuToRemove);
                }

                _NarijeDBContext.MenuInfo.Update(MenuInfo);
                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                {
                    await transaction.RollbackAsync();
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");
                }

                await transaction.CommitAsync();
                return await GetAsync(MenuInfo.Id);
            }
            catch (Exception Ex)
            {
                await transaction.RollbackAsync();
                return new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: Ex.InnerException != null ? Ex.InnerException.Message : Ex.Message);
            }
        }

        private async Task UpdateReservesAndWallet(int menuId, int oldPrice, int newPrice, EnumFoodType foodType)
        {
            var reserves = await _NarijeDBContext.Reserves
                .Include(r => r.Food)
                .Where(r => r.MenuId == menuId && r.FoodType == (int)foodType && r.PayType == (int)EnumInvoicePayType.debit)
                .ToListAsync();

            foreach (var reserve in reserves)
            {
                var user = await _NarijeDBContext.Users.FindAsync(reserve.UserId);
                if (user != null)
                {
                    long wallet = await GetWalletBalance(_NarijeDBContext, user, reserve.PayType);
                    int priceDiff = Math.Abs(oldPrice - newPrice);
                    int operation = oldPrice > newPrice ? (int)EnumWalletOp.Revoke : (int)EnumWalletOp.Debit;
                    string msg = operation == (int)EnumWalletOp.Debit
                      ? $"کم کردن وجه از کیف پول بابت تغییر قیمت غذای '{reserve.Food.Title}': {oldPrice} به قیمت: {newPrice}"
                      : $"عودت وجه به کیف پول بابت تغییر قیمت غذای '{reserve.Food.Title}': {oldPrice} به قیمت: {newPrice}";


                    long newWalletValue = operation == (int)EnumWalletOp.Debit
                        ? wallet - priceDiff
                        : wallet + priceDiff;

                    await ProcessWallet(_NarijeDBContext, user, wallet, priceDiff, operation, newWalletValue, msg);
                }
            }
        }

        private async Task UpdateReservePricesRawAsync(List<int> reserveIds, int newPrice)
        {
            if (reserveIds == null || reserveIds.Count == 0)
                return;

            const int batchSize = 500;
            for (int i = 0; i < reserveIds.Count; i += batchSize)
            {
                var batch = reserveIds.Skip(i).Take(batchSize).ToList();
                var idList = string.Join(",", batch);
                var sql = $"UPDATE [dbo].[Reserve] SET [Price] = @p0 WHERE [id] IN ({idList})";
                await _NarijeDBContext.Database.ExecuteSqlRawAsync(sql, new object[] { newPrice });
            }
        }

        private async Task DeleteReserve(Menu menu, Reserve reserve)
        {

            var user = await _NarijeDBContext.Users.FindAsync(reserve.UserId);
            if (user != null)
            {
                long wallet = await GetWalletBalance(_NarijeDBContext, user, reserve.PayType);
                int priceDiff = Math.Abs(reserve.Price);
                int operation = (int)EnumWalletOp.Revoke;
                string msg = $"عودت وجه به کیف پول بابت حذف از منو  غذای '{reserve.Food.Title}";


                long newWalletValue = wallet + priceDiff;

                await ProcessWallet(_NarijeDBContext, user, wallet, priceDiff, operation, newWalletValue, msg);
            }
        }
        // برسی اعتبار کیف پول کاربر
        private async Task<long> GetWalletBalance(NarijeDBContext dbContext, User user, int payType)
        {
            if (payType == 1)
            {
                return await dbContext.Wallets
                    .Where(w => w.UserId == user.Id)
                    .OrderByDescending(w => w.Id)
                    .Select(w => w.RemValue)
                    .FirstOrDefaultAsync();
            }
            return 0;
        }
        private async Task<string> ProcessWallet(NarijeDBContext dbContext, User user, long wallet, long price, int op, long value, string msg)
        {

            Wallet w = new Wallet()
            {
                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                Op = op,
                Value = price,
                UserId = user.Id,
                PreValue = wallet,
                RemValue = value,
                Description = msg
            };
            w.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(w);
            await dbContext.Wallets.AddAsync(w);

            WalletPayment wp = new WalletPayment()
            {
                Status = 1,
                UserId = user.Id,
                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                Op = op,
                Value = price,
                Gateway = (int)EnumGateway.Wallet,
                Wallet = w,
                Description = msg
            };
            await dbContext.WalletPayments.AddAsync(wp);



            return "";
        }
        #endregion
        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Menu = await _NarijeDBContext.MenuInfo
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Menu is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.MenuInfo.Remove(Menu);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion


        #region EditActiveAsync
        // ------------------
        //  EditActiveAsync
        // ------------------
        public async Task<ApiResponse> EditActiveAsync(int id)
        {
            var Data = await _NarijeDBContext.MenuInfo
                                                  .Where(A => A.Id == id)
                                                  .FirstOrDefaultAsync();
            if (Data is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Data.Active = !Data.Active;

            _NarijeDBContext.MenuInfo.Update(Data);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Data.Id);
        }
        #endregion


        #region ImportFromExcel
        public async Task<ApiResponse> ImportFromExcelAsync(IFormFile file, MenuInfoEditRequest request, int MealType)
        {
            using (var transaction = await _NarijeDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var MenuInfoId = 0;
                    if (request.id != 0)
                    {
                        var MenuInfo = await _NarijeDBContext.MenuInfo
                                                      .Where(A => A.Id == request.id)
                                                      .FirstOrDefaultAsync();
                        MenuInfoId = MenuInfo.Id;
                    }
                    else
                    {

                        var MenuInfo = new MenuInfo()
                        {
                            Title = request.title,
                            Description = request.description,
                            Month = request.month,
                            Year = request.year,
                            Active = request.active,

                        };

                        await _NarijeDBContext.MenuInfo.AddAsync(MenuInfo);

                        var Result = await _NarijeDBContext.SaveChangesAsync();

                        if (Result < 0)
                            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

                        MenuInfoId = MenuInfo.Id;
                    }
                    var updatedMenus = new List<Menu>();
                    var newMenus = new List<Menu>();
                    var errors = new List<string>();
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var stream = new MemoryStream())
                    {
                        await file.CopyToAsync(stream);
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var rowCount = worksheet.Dimension?.Rows ?? 0;
                            var colCount = worksheet.Dimension?.Columns ?? 0;

                            if (rowCount < 2)
                            {
                                return new ApiErrorResponse(
                                    _Code: StatusCodes.Status400BadRequest,
                                    _Message: "فرمت و دیتا فایل اکسل درست نمی باشد"
                                );
                            }

                            var categoryGroups = new List<(int StartCol, int EndCol)>();
                            for (int col = 1; col <= colCount; col++)
                            {
                                var headerText = worksheet.Cells[1, col].Text?.Trim();
                                if (headerText == "ایدی کالا")
                                {
                                    int startCol = col;
                                    int endCol = col + 1;
                                    while (endCol <= colCount)
                                    {
                                        var nextHeader = worksheet.Cells[1, endCol].Text?.Trim();
                                        if (nextHeader == "ایدی کالا")
                                            break;
                                        endCol++;
                                    }
                                    categoryGroups.Add((startCol, endCol - 1));
                                    col = endCol - 1;
                                }
                            }

                            if (!categoryGroups.Any())
                            {
                                return new ApiErrorResponse(
                                    _Code: StatusCodes.Status400BadRequest,
                                    _Message: "ستون گروه کالا یافت نشد"
                                );
                            }


                            try

                            {
                                for (int row = 2; row <= rowCount; row++)
                                {
                                    var dateStr = worksheet.Cells[row, 2].Text?.Trim();
                                    if (string.IsNullOrEmpty(dateStr))
                                        continue;

                                    DateTime parsedDate;

                                    var persianCulture = new CultureInfo("fa-IR");
                                    persianCulture.DateTimeFormat.Calendar = new PersianCalendar();

                                    if (!DateTime.TryParseExact(dateStr, "dd/MM/yyyy", persianCulture, DateTimeStyles.None, out parsedDate))
                                    {
                                        errors.Add($"این تاریخ نا معتبر می باشد در سطر :‌ {row}: {dateStr}");
                                        continue;
                                    }


                                    var utcDateTime = new DateTime(
      parsedDate.Year,
      parsedDate.Month,
      parsedDate.Day,
      0, 0, 0,
      DateTimeKind.Utc);
                                    var seenFoodEntries = new HashSet<(DateTime, int, int)>();

                                    foreach (var group in categoryGroups)
                                    {
                                        var categoryIdCol = group.StartCol;
                                        var foodIdCol = -1;
                                        var maxReserveCol = -1;
                                        var echoPrice = 0;
                                        var specialPrice = 0;
                                        var arpaNumber = 0;
                                        var persianCalendar = new PersianCalendar();
                                        var dateParts = dateStr.Split('/');


                                        for (int col = group.StartCol; col <= group.EndCol; col++)
                                        {
                                            var header = worksheet.Cells[1, col].Text?.Trim();
                                            if (header == "ایدی کالا")
                                                foodIdCol = col;
                                            else if (header == "حداکثر رزرو")
                                                maxReserveCol = col;
                                            else if (header == "قیمت پایه")
                                            {
                                                specialPrice = col;
                                                echoPrice = col;
                                            }

                                            else if (header == "کد کالا")
                                                arpaNumber = col;
                                        }

                                        if (foodIdCol == -1 || maxReserveCol == -1)
                                        {
                                            errors.Add($"ستون وارد شده معتبر نمی باشد :‌ {group.StartCol}");
                                            continue;
                                        }


                                        var echoPriceValue = worksheet.Cells[row, echoPrice].Text?.Trim();
                                        var specialPriceValue = worksheet.Cells[row, specialPrice].Text?.Trim();

                                        var foodIdStr = worksheet.Cells[row, foodIdCol].Text?.Trim();

                                        var arpaNumberStr = worksheet.Cells[row, arpaNumber].Text?.Trim();
                                        int? foodId = int.TryParse(foodIdStr, out var parsedFoodId) ? parsedFoodId : (int?)null;

                                        Food? food = null;

                                        if (!string.IsNullOrEmpty(arpaNumberStr))
                                        {
                                            food = await _NarijeDBContext.Foods
                                                .FirstOrDefaultAsync(f => f.ArpaNumber == arpaNumberStr);
                                        }
                                        else if (foodId.HasValue)
                                        {
                                            food = await _NarijeDBContext.Foods
                                                .FirstOrDefaultAsync(f => f.Id == foodId.Value);
                                        }
                                        else if (foodId.HasValue == false && string.IsNullOrEmpty(arpaNumberStr))
                                        {
                                            continue;
                                        }

                                        if (food == null)
                                        {
                                            var foodNameCol = foodIdCol + 1;
                                            errors.Add($" کالا وارد شده در لیست کالا ها موجود نمی باشد سطر‌:‌ {row}, تاریخ : {dateStr},  آیدی یا کد کالا : {foodId ?? arpaNumber}");
                                            continue;
                                        }

                                        var maxReserveStr = worksheet.Cells[row, maxReserveCol].Text?.Trim();
                                        if (!int.TryParse(maxReserveStr, out int maxReserve))
                                        {
                                            errors.Add($"فرمت حداکثر رزرو غذا اشتباه است  {row}, ستون :‌ {maxReserveCol}: {maxReserveStr}");
                                            continue;
                                        }



                                        var entryKey = (utcDateTime, MealType, food.Id);
                                        if (seenFoodEntries.Contains(entryKey))
                                        {
                                            errors.Add($"این کالا در همین روز و همین وعده تکراری است: سطر {row}, تاریخ: {dateStr},  آیدی کالا: {food.Id}");
                                            continue;
                                        }

                                        seenFoodEntries.Add(entryKey);



                                        var menu = new Menu
                                        {
                                            DateTime = utcDateTime,
                                            FoodType = 0,
                                            FoodId = food.Id,
                                            MaxReserve = maxReserve,
                                            MenuInfoId = MenuInfoId,
                                            MealType = MealType,
                                            EchoPrice = int.Parse(echoPriceValue),
                                            SpecialPrice = int.Parse(specialPriceValue)
                                        };
                                        await _NarijeDBContext.Menus.AddAsync(menu);
                                        newMenus.Add(menu);

                                    }
                                }

                                if (errors.Any())
                                {
                                    await transaction.RollbackAsync();
                                    return new ApiErrorResponse(
                                        _Code: StatusCodes.Status400BadRequest,
                                        _Message: errors[0]
                                    );
                                }

                                var result = await _NarijeDBContext.SaveChangesAsync();
                                if (result == 0)
                                {
                                    await transaction.RollbackAsync();
                                    return new ApiErrorResponse(
                                        _Code: StatusCodes.Status500InternalServerError,
                                        _Message: "خطای سرور لطفا مجدد تلاش کنید"
                                    );
                                }

                                await transaction.CommitAsync();
                                return new ApiOkResponse(_Message: "SUCCESS", _Data: new
                                {
                                    Updated = updatedMenus.Count,
                                    New = newMenus.Count,
                                    id = MenuInfoId
                                });
                            }
                            catch (Exception ex)
                            {

                                throw;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: ex.Message);
                }

            }
        }
        #endregion
    }
}