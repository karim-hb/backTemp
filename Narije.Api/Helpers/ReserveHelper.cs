using Microsoft.AspNetCore.Http;
using Narije.Infrastructure.Contexts;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;
using Narije.Core.DTOs.User;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Narije.Core.DTOs.Enum;
using Narije.Core.Entities;
using System.Linq;
using Narije.Core.DTOs.Public;
using System.Text.RegularExpressions;
using Narije.Api.Payment.AsanPardakht.models.bill;
using Castle.Core.Resource;
using Narije.Core.DTOs.Admin;
using System.Threading;
using Castle.DynamicProxy.Generators;
using System.Drawing;
using System.Diagnostics;
using Org.BouncyCastle.Utilities;
using System.Globalization;
using System.IO;

namespace Narije.Api.Helpers
{
    public class ReserveHelper
    {
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);



        public async Task<List<ReservesResponse>> GetUserReserve(NarijeDBContext _NarijeDBContext, User user, DateTime StartWeek, DateTime EndWeek, string search, int? groupId, string BucketServiceURL, int ReserveType, int mealId, int month, int year)
        {
            var priceHelper = new PriceHelper();

            var E = _NarijeDBContext.vReserves
              .Where(A => A.UserId == user.Id && A.DateTime.Date >= StartWeek.Date && A.DateTime.Date <= EndWeek.Date && A.MealType == mealId)
              .AsNoTracking();
            switch (ReserveType)
            {
                case (int)EnumReserveState.normal:
                case (int)EnumReserveState.guest:
                case (int)EnumReserveState.canceled:
                case (int)EnumReserveState.admin:
                    E = E.Where(A => A.State == ReserveType);
                    break;
            }
            var UserReserves = await E.ToListAsync();
            int ftype = (int)EnumFoodType.special;
            if (user.Customer != null)
                ftype = user.Customer.FoodType;
            List<ReservesResponse> reserves = new();



            var CId = user.CustomerId;
            // برسی اینکه این وعده فعال است یا نه 
            var mealInfo = await _NarijeDBContext.CompanyMeal.Where(c => c.CustomerId == CId && c.MealId == mealId && c.Active == true).Select(c => new { Id = c.Id }).FirstOrDefaultAsync();
            if (mealInfo == null)
                return new List<ReservesResponse>();

            var branch = await _NarijeDBContext.Customers.Where(c => c.Id == CId).Select(c => new { Id = c.Id, ParentId = c.ParentId }).FirstOrDefaultAsync();

            if (branch == null)
                return new List<ReservesResponse>();

            var customer = await _NarijeDBContext.Customers.Where(c => c.Id == branch.ParentId).FirstOrDefaultAsync();
            if (customer == null)
                return new List<ReservesResponse>();

            // دریافت اطلاعات از منو های فعال
            var menuInfo = await _NarijeDBContext.CustomerMenuInfo.Where(c => c.CustomerId == customer.Id && c.Month == month && c.Year == year && c.MenuInfo.Active == true).Select(c => new { Id = c.MenuInfoId }).FirstOrDefaultAsync();
            if (menuInfo == null)
                return new List<ReservesResponse>();

            var CustomerHasMenu = await _NarijeDBContext.Menus
              .Where(A => A.MenuInfoId == menuInfo.Id && A.DateTime.Date == StartWeek.Date && A.MealType == mealId)
              .AsNoTracking()
              .CountAsync();

            while (StartWeek <= EndWeek)
            {



                var menus = await _NarijeDBContext.Menus
                     .Where(A => A.MenuInfoId == menuInfo.Id && A.DateTime.Date == StartWeek.Date && A.MealType == mealId && A.MaxReserve > 0)
                     .ToListAsync();

                var Q = await Task.WhenAll(menus.Select(async A => new ReserveResponse()
                {
                    id = A.Id,
                    maxReserve = A.MaxReserve,
                    foodId = A.FoodId,
                    food = A.Food.Title,
                    foodDescription = A.Food.Description,
                    foodGroupId = A.Food.GroupId,
                    foodType = ftype,
                    foodGroup = A.Food.Group.Title,
                    image = A.Food.Gallery == null ? "" : $"{BucketServiceURL}{A.Food.GalleryId}",
                    state = "",
                    price = await priceHelper.GetPriceForMenu(customer, A,A.Food.Vip),
                    echoPrice = A.EchoPrice ?? A.Food.EchoPrice,
                    specialPrice = A.SpecialPrice ?? A.Food.SpecialPrice,
                    qty = 0,
                    mealType = A.MealType,
                    isFood = A.Food.IsFood,
                    hasSurvey = false
                }));

                if (groupId != null)
                    Q = Q.Where(A => A.foodGroupId == groupId).ToArray();

                if (search != null)
                    Q = Q.Where(A => A.food.Contains(search)).ToArray();

                var items = Q.ToList();

                var ids = items.Select(A => A.foodId).ToList();
                var Foods = _NarijeDBContext.Foods.Where(A => ids.Contains(A.Id)).ToList();

                foreach (var item in items)
                {

                    var r = UserReserves
                      .Where(A => A.DateTime.Date == StartWeek.Date && A.FoodId == item.foodId && A.MealType == mealId && A.Num > 0)
                      .FirstOrDefault();
                    if (r != null)
                    {
                        item.state = EnumHelper<EnumReserveState>.GetDisplayValue((EnumReserveState)r.State);
                        item.qty = r.Num;
                        item.price = r.Price;
                        item.foodType = r.FoodType;
                        item.hasSurvey = r.HasSurvey;
                        item.reserveId = r.Id;
                    }


                }

                reserves.Add(new ReservesResponse()
                {
                    datetime = StartWeek,
                    reserves = items
                });

                StartWeek = StartWeek.AddDays(1).Date;
            }

            return reserves;
        }

        public async Task<string> Reserve(NarijeDBContext _NarijeDBContext, User user, ReservesRequest day, int reserveType, bool isAdmin)
        {
            await semaphore.WaitAsync();
            try
            {
                //برسی فعال بودن شرکت
                if (!user.Customer.Active)
                    return "امکان رزرو برای شرکت شما وجود ندارد";

                // برسی این که همزمان نقدی و اعتباری خرید نکنه
                var userReserves = await GetUserReserves(_NarijeDBContext, user, day.datetime.Date, reserveType, day.mealId);

                var distinctPayTypes = userReserves.Select(x => x.PayType).Distinct().ToList();
                if (distinctPayTypes.Count > 1)
                {
                    return "امکان عودت یا رزرو برای دو مدل پرداختی وجود ندارد";
                }




                // برسی تاریخ مجاز برای رزرو 
                var CustomerReserveTime = await _NarijeDBContext.CompanyMeal.Where(c => c.CustomerId == user.CustomerId && c.MealId == day.mealId).FirstOrDefaultAsync();

                if (CustomerReserveTime == null)
                    return "وعده غذایی مورد نظر برای این شرکت تعریف نشده است";
                {

                }

                var diff = day.datetime.Date - TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")).Date;
                var reservationDateTime = day.datetime.Date;
                if (CustomerReserveTime != null)
                {
                    TimeSpan maxReserveTime = TimeSpan.Parse(CustomerReserveTime.MaxReserveTime);
                    reservationDateTime += maxReserveTime;
                }
                var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                DateTime tomorrow = currentTime.AddDays(1);
                if (reservationDateTime < tomorrow)
                    return "زمان رزرو برای روز انتخاب شده به پایان رسیده است ";

                if (diff.Days > user.Customer.ReserveTo)
                {
                    return "این روز خارج از محدوده مجاز رزرو است";
                }
                if (diff.Days == 0)
                    if (TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")).TimeOfDay > user.Customer.ReserveTime)
                    {
                        return "زمان مجاز رزور غذا سپری شده است";
                    }


                // برسی عنواع غذا های رزروی
                if (!isAdmin)
                {
                    var distinctMealTypes = await _NarijeDBContext.Reserves
                  .Where(r => r.UserId == user.Id && r.DateTime.Date == day.datetime.Date && r.ReserveType == reserveType && r.Num > 0)
                  .Select(r => r.MealType)
                  .Distinct()
                  .ToListAsync();

                    if (!distinctMealTypes.Contains(day.mealId))
                    {
                        distinctMealTypes.Add(day.mealId);
                    }

                    // بررسی شرط محدودیت تعداد انواع وعده
                    if (distinctMealTypes.Count > user.Customer.MaxMealCanReserve)
                    {
                        return "حداکثر وعده مجاز را رزرو کرده‌اید.";
                    }
                    var menus = await GetMenu(_NarijeDBContext, user, day);


                    foreach (var item in day.reserves)
                    {

                        var menu = menus.Where(A => A.foodId == item.foodId).FirstOrDefault();

                        if (menu is null)
                        {
                            return "غذای انتخابی در منو وجود ندارد";
                        }



                        if (item.qty > menu.maxReserve)
                        {
                            return "امکان رزرو این تعداد از غذا را ندارید";
                        }


                    }




                }
                var tehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
                var tehranTime = TimeZoneInfo.ConvertTime(day.datetime, tehranTimeZone);

                var persianCalendar = new System.Globalization.PersianCalendar();
                var shamsiDayOfWeek = persianCalendar.GetDayOfWeek(tehranTime);

                int selectedBranch = 0;

                switch (shamsiDayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        selectedBranch = user.Customer.BranchForSaturday ?? 0;
                        break;
                    case DayOfWeek.Sunday:
                        selectedBranch = user.Customer.BranchForSunday ?? 0;
                        break;
                    case DayOfWeek.Monday:
                        selectedBranch = user.Customer.BranchForMonday ?? 0;
                        break;
                    case DayOfWeek.Tuesday:
                        selectedBranch = user.Customer.BranchForTuesday ?? 0;
                        break;
                    case DayOfWeek.Wednesday:
                        selectedBranch = user.Customer.BranchForWednesday ?? 0;
                        break;
                    case DayOfWeek.Thursday:
                        selectedBranch = user.Customer.BranchForThursday ?? 0;
                        break;
                    case DayOfWeek.Friday:
                        selectedBranch = user.Customer.BranchForFriday ?? 0;
                        break;

                    default:
                        selectedBranch = user.Customer.BranchForFriday ?? 0;
                        break;
                }




                if (selectedBranch == 0)
                {
                    return "شرکت خدمات دهنده برای شرکت شما مشخص نشده است لطفا با پشتیبانی تماس بگرید ";
                }







                int payType = user.Customer.PayType;
                long wallet = await GetWalletBalance(_NarijeDBContext, user, payType);

                // باید حتما رزرو قبلی از طریق کیف پول پرداخت شده باشه تا بشه عودت داد به کیف پول
                bool canWithrawToWalletForThisReserve = distinctPayTypes.Count == 1 && distinctPayTypes[0] == (int)EnumInvoicePayType.debit && payType == (int)EnumInvoicePayType.debit;

                return await ProcessReservation(_NarijeDBContext, user, day, reserveType, isAdmin, payType, wallet, canWithrawToWalletForThisReserve, selectedBranch, CustomerReserveTime);


            }
            finally
            {
                semaphore.Release();
            }
        }

        #region GetWalletBalance
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

        #endregion


        #region ProcessReservation
        // مدیریت ترانس اکشن دیتابیس و خطاها 
        private async Task<string> ProcessReservation(
            NarijeDBContext dbContext,
            User user,
            ReservesRequest day,
            int reserveType,
            bool isAdmin,
            int payType,
            long wallet,
            bool canWithrawToWalletForThisReserve,
            int selectedBranch,
            CompanyMeal CustomerReserveTime
            )
        {


            var mealType = day.mealId;
            var userReserves = await GetUserReserves(dbContext, user, day.datetime.Date, reserveType, mealType);

            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                var customerReserveTime = user.Customer.ReserveTime;


                var subsidy = userReserves.Where(r => r.DateTime.Date == day.datetime.Date).Sum(r => r.Subsidy);
                var reserveResult = await ReserveDay(dbContext, user, day, reserveType, wallet, isAdmin, payType, subsidy, userReserves, canWithrawToWalletForThisReserve, selectedBranch, CustomerReserveTime);

                if (!string.IsNullOrEmpty(reserveResult))
                {
                    await transaction.RollbackAsync();
                    return reserveResult;
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return $"خطای سرور : {msg}";
                throw;
            }
            return "";

        }

        #endregion

        #region GetUserReserves
        // دریافت رزرو کاربر بر اساس روز وعده .و نوع رزرو
        private async Task<List<Reserve>> GetUserReserves(NarijeDBContext dbContext, User user, DateTime date, int reserveType, int mealType)
        {
            return await dbContext.Reserves
                .Where(r => r.UserId == user.Id && r.DateTime.Date == date && r.ReserveType == reserveType && r.MealType == mealType && r.Num > 0)
                .ToListAsync();
        }

        #endregion

        #region GetCustomerPredictedReserve
        // دریافت رزرو های پیش بینی شده توسط ادمین برای این شرکت 
        private async Task<List<Reserve>> GetCustomerPredictedReserve(NarijeDBContext dbContext, User user, DateTime date, int reserveType, int mealType)
        {
            return await dbContext.Reserves
                .Where(r => r.CustomerId == user.CustomerId && r.DateTime.Date == date && r.ReserveType == reserveType && r.MealType == mealType)
                .ToListAsync();
        }

        #endregion

        #region ReserveDay
        // برسی منو و غذا رزروی کاربر
        private async Task<string> ReserveDay(
             NarijeDBContext dbContext,
             User user,
             ReservesRequest day,
             int reserveType,
             long wallet,
             bool isAdmin,
             int payType,
             int subsidy,
             List<Reserve> userReserves,
             bool canWithrawToWalletForThisReserve,
             int selectedBranch,
               CompanyMeal CustomerReserveTime


     )
        {

            long credit = 0;
            var menus = await GetMenu(dbContext, user, day);



            foreach (var item in day.reserves)
            {
                var reserveResult = await ProcessReserveItem(dbContext, user, day, reserveType, payType, subsidy, userReserves, item, menus, canWithrawToWalletForThisReserve, wallet, selectedBranch, isAdmin, CustomerReserveTime);

                if (!string.IsNullOrEmpty(reserveResult))
                    return reserveResult;
            }

            // حذف غذا های پیش بینی شده برای امروز پس از ثبت رزرو واقعی
          //  var perdictReserve = await GetCustomerPredictedReserve(dbContext, user, day.datetime.Date, (int)EnumReserveState.perdict, day.mealId);
          //  if (perdictReserve.Any())
         //   {
         //       dbContext.Reserves.RemoveRange(perdictReserve);
        //    }

            // حذف غذا های حذف شده از لیست رزرو کاربر
            var ids = day.reserves.Select(A => A.foodId).ToList();
            var mustDeleted = userReserves.Where(A => !ids.Contains(A.FoodId)).ToList();
            if (mustDeleted.Count > 0)
            {
                if (canWithrawToWalletForThisReserve)
                {


                    foreach (var reserve in mustDeleted)
                    {
                        credit += reserve.Price * reserve.Num;
                    }
                    if (credit > 0)
                    {
                        try
                        {
                            var msg = $"  عودت بابت سفارش حذف شده :‌  {mustDeleted[0].Id}  , غذا :‌ {mustDeleted[0].Food.Title}";
                            var walletResult = await ProcessWallet(dbContext, user, wallet, credit, (int)EnumWalletOp.Revoke, wallet + credit, msg);
                            if (!string.IsNullOrEmpty(walletResult))
                                return walletResult;
                        }
                        catch (Exception ex) { }
                    }

                }


                dbContext.Reserves.RemoveRange(mustDeleted);
            }


            return "";
        }

        #endregion

        #region GetMenuInfo


        private async Task<int> GetMenuInfo(NarijeDBContext dbContext, User user, DateTime day)
        {

            var customerId = user.CustomerId;

            var persianCalendar = new PersianCalendar();
            int year = persianCalendar.GetYear(day);
            int month = persianCalendar.GetMonth(day);

            var branch = await dbContext.Customers.Where(c => c.Id == customerId).Select(c => new { Id = c.Id, ParentId = c.ParentId }).FirstOrDefaultAsync();

            var customer = await dbContext.Customers.Where(c => c.Id == branch.ParentId).Select(c => new { Id = c.Id }).FirstOrDefaultAsync();

            var menuInfo = await dbContext.CustomerMenuInfo.Where(c => c.CustomerId == customer.Id && c.Month == month && c.Year == year).Select(c => new { Id = c.MenuInfoId }).FirstOrDefaultAsync();

            return menuInfo.Id;

        }

        #endregion

        #region GetMenu 
        // دریافت منو در صورتی که شرکت کاربر منو داشته باشه منو کاربر استفاده میشه در غیر این صورت منو پدر
        private async Task<List<ReserveResponse>> GetMenu(NarijeDBContext dbContext, User user, ReservesRequest day)
        {

            var menuInfoId = await GetMenuInfo(dbContext, user, day.datetime.Date);

            var menus = await dbContext.Menus
                .Where(m => m.MenuInfoId == menuInfoId && m.DateTime.Date == day.datetime.Date)
                .Select(m => new ReserveResponse
                {
                    maxReserve = m.MaxReserve,
                    foodId = m.FoodId,
                    mealType = m.MealType,
                    food = m.Food.Title,
                    foodDescription = m.Food.Description,
                    foodGroupId = m.Food.GroupId,
                    foodGroup = m.Food.Group.Title,
                    echoPrice = m.EchoPrice ?? m.Food.EchoPrice,
                    specialPrice = m.SpecialPrice ?? m.Food.SpecialPrice,
                    id = m.Id,
                })
                .ToListAsync();

            return menus;
        }


        #endregion



        #region ValidateFoodSelection
        // برسی و ولدیشن غذا ها 
        private async Task<bool> ValidateFoodSelection(List<ReserveHelperFoodRequest> foods, ReservesRequest day)
        {
            var selectedFoodIds = day.reserves.Where(r => r.qty > 0).Select(r => r.foodId).ToList();
            return foods.Count(f => selectedFoodIds.Contains(f.Id) && f.isFood) <= 1;
        }
        #endregion

        #region ProcessReserveItem
        //ثبت یا ویرایش رزرو کاربر
        private async Task<string> ProcessReserveItem(
            NarijeDBContext dbContext,
            User user,
            ReservesRequest day,
            int reserveType,
            int payType,
            int subsidy,
            List<Reserve> userReserves,
            ReserveRequest item,
            List<ReserveResponse> menus,
            bool canWithrawToWalletForThisReserve,
            long wallet,
            int selectedBranch,
            bool isAdmin,
             CompanyMeal CustomerReserveTime

            )
        {
            var menu = menus.FirstOrDefault(m => m.foodId == item.foodId);
            if (menu == null) return "غذا انتخاب شده موجود نیست";
            var priceHelper = new PriceHelper();
            var customer = await dbContext.Customers.Where(c => c.Id == user.Customer.ParentId).FirstOrDefaultAsync();
            var food = await dbContext.Foods.Where(c => c.Id == item.foodId).FirstOrDefaultAsync();
            int price = await priceHelper.GetPriceForMenu(customer, menu, food.Vip);
           
            var menuInfoId = await GetMenuInfo(dbContext, user, day.datetime.Date);

            var existingReserve = userReserves.FirstOrDefault(r => r.DateTime.Date == day.datetime.Date && r.FoodId == item.foodId);

            if (existingReserve == null)
            {
                int usedSubsidy = Math.Min(subsidy, price * item.qty);
                subsidy -= usedSubsidy;
                await dbContext.Reserves.AddAsync(new Reserve
                {
                    CreatedAt = DateTime.UtcNow,
                    CustomerId = user.CustomerId.Value,
                    Num = item.qty,
                    UserId = user.Id,
                    DateTime = day.datetime.Date,
                    ReserveType = 0,
                    State = reserveType,
                    FoodId = item.foodId,
                    MealType = day.mealId,
                    FoodType = user.Customer.FoodType,
                    Price = price,
                    Subsidy = usedSubsidy,
                    PayType = payType,
                    MenuId = menu.id,
                    MenuInfo = menuInfoId,
                    BranchId = selectedBranch,
                    PriceType = customer.PriceType,
                    DeliverHour = CustomerReserveTime.DeliverHour,


                });

                if (user.Customer.PayType == (int)EnumInvoicePayType.debit)
                {
                    var foodTitle = dbContext.Foods.FirstOrDefault(f => f.Id == item.foodId).Title;
                    var changePrice = item.qty * price;
                    var msg = $" مصرف بابت ثبت رزرو  :‌  {foodTitle}";
                    var walletResult = await ProcessWallet(dbContext, user, wallet, changePrice, (int)EnumWalletOp.Debit, wallet - changePrice, msg);
                    if (!string.IsNullOrEmpty(walletResult))
                        return walletResult;
                }

                return "";
            }
            else
            {
                if (item.qty > 0 && item.qty > menu.maxReserve && !isAdmin)
                {
                    return "محدودیت رزرو تعداد ";
                }

                var foodTitle = dbContext.Foods.FirstOrDefault(f => f.Id == item.foodId).Title;

                var difference = existingReserve.Num - item.qty;
                //در صورت که تعداد غذا کاهش پیدا کنه عودت وگر نه مبلغ از کیف پول کم میشود
                if (difference > 0 && canWithrawToWalletForThisReserve)
                {
                    var msg = $" عودت بابت ویرایش رزرو به شماره   :‌  {existingReserve.Id}   و غذایه :‌ {foodTitle}";
                    var changePrice = existingReserve.Price * difference;
                    var walletResult = await ProcessWallet(dbContext, user, wallet, changePrice, (int)EnumWalletOp.Revoke, wallet + changePrice, msg);
                    if (!string.IsNullOrEmpty(walletResult))
                        return walletResult;

                }
                else if (user.Customer.PayType == (int)EnumInvoicePayType.debit && difference < 0)
                {
                    var msg = $" مصرف بابت ویرایش رزرو به شماره   :‌  {existingReserve.Id}   و غذایه :‌ {foodTitle}";
                    var changePrice = existingReserve.Price * (difference * -1);
                    var walletResult = await ProcessWallet(dbContext, user, wallet, changePrice, (int)EnumWalletOp.Debit, wallet - changePrice, msg);
                    if (!string.IsNullOrEmpty(walletResult))
                        return walletResult;
                }
                existingReserve.Num = item.qty;
                existingReserve.PayType = payType;
                existingReserve.PriceType = user.Customer.PriceType;
                dbContext.Reserves.Update(existingReserve);
            }

            return "";
        }

        #endregion

        #region ProcessWallet
        // مدیریت کیف پول انجام تراکنش خرید یا عودت 
        private async Task<string> ProcessWallet(NarijeDBContext dbContext, User user, long wallet, long price, int op, long value, string msg)
        {
            if (wallet < price && op != (int)EnumWalletOp.Revoke)
                return "موجودی کیف پول شما ناکافی می باشد";
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



    }
}