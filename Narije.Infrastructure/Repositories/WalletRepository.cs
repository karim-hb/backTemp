using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Wallet;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Narije.Core.DTOs.Enum;
using System.Reflection.PortableExecutable;
using Narije.Core.DTOs.ViewModels.Export;
using System.Globalization;
using System.Web;
using System.Security.Principal;
using TM.Core.DTOs.Enum;
using System.Diagnostics;
using System.Drawing;

namespace Narije.Infrastructure.Repositories
{
    public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public WalletRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        public async Task<ApiResponse> ExportWalletAsync()
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Wallet");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Wallets
                .ProjectTo<WalletResponse>(_IMapper.ConfigurationProvider);

            var culture = CultureInfo.InvariantCulture;

            var fdate = query.Filter.FirstOrDefault(a => a.Key.Equals("dateTime") && a.Operator.Equals("ge"));
            if (fdate != null)
            {
                string dateString = fdate.Value.Replace("\"", ""); // Remove any unnecessary characters
                string[] dateTimeFormats = { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff" }; // Add any additional formats if needed

                if (DateTime.TryParseExact(dateString, dateTimeFormats,
                        CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out DateTime parsedDate))
                {
                    fromDate = parsedDate;
                    query.Filter.Remove(fdate);
                }
                else
                {
                    throw new ArgumentException("فرمت فیلتر تاریخ شروع نامعتبر است");
                }
            }

            var tdate = query.Filter.FirstOrDefault(a => a.Key.Equals("dateTime") && a.Operator.Equals("le"));
            if (tdate != null)
            {
                string dateString = tdate.Value.Replace("\"", ""); // Remove any unnecessary characters
                string[] dateTimeFormats = { "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fff" }; // Add any additional formats if needed

                if (DateTime.TryParseExact(dateString, dateTimeFormats,
                        CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out DateTime parsedDate))
                {
                    toDate = parsedDate;
                    query.Filter.Remove(tdate);
                }
                else
                {
                    throw new ArgumentException("فرمت فیلتر تاریخ پایان نامعتبر است");
                }
            }

            if (fromDate is not null)
                Q = Q.Where(a => a.dateTime.Date >= fromDate.Value.Date);
            if (toDate is not null)
                Q = Q.Where(a => a.dateTime.Date <= toDate.Value.Date);

            var param = HttpUtility
                .ParseQueryString(_IHttpContextAccessor.HttpContext.Request.QueryString.ToString());
            var key = param.AllKeys.FirstOrDefault(a => a is "ids");
            if (key != null)
            {
                var ids = param[key];
                if (!string.IsNullOrEmpty(ids))
                {
                    var targetIds = ids.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                    Q = Q.Where(a => targetIds.Contains(a.id));
                }
            }

            var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();

            if (user is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

            switch (user.Role)
            {
                case (int)EnumRole.user:
                    Q = Q.Where(A => A.userId == user.Id);
                    break;
                case (int)EnumRole.customer:
                    Q = Q.Where(A => A.customerId == user.CustomerId);
                    break;
            }


            var report = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            foreach (var item in Q)
            {
                if (item.pan is { Length: 16 })
                {
                    item.pan = item.pan.Remove(4, 8).Insert(4, "xxxxxxxx");
                }
            }

            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            result.header = header.Select(a => a.title).ToList();

            var data = await report.ToListAsync<object>();

            result.body = ExportHelper.MakeResult(data, header, false);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Wallet = await _NarijeDBContext.Wallets
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<WalletResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Wallet);
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

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Wallet");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.Wallets
                            .ProjectTo<WalletResponse>(_IMapper.ConfigurationProvider);

                var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((Identity is null) || (Identity.Claims.Count() == 0))
                    return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

                var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();

                if (user is null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

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

                var Wallets = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

                foreach (var item in Wallets.Data)
                {
                    if ((item.pan != null) && (item.pan.Length == 16))
                    {
                        item.pan = item.pan.Remove(4, 8).Insert(4, "xxxxxxxx");
                    }
                }

                return new ApiOkResponse(_Message: "SUCCESS", _Data: Wallets.Data, _Meta: Wallets.Meta, _Header: header);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);

            }


        }
        #endregion

        #region GetSumAsync
        // ------------------
        //  GetSumAsync
        // ------------------
        public async Task<ApiResponse> GetSumAsync()
        {
            #region Create Header Response
            List<FieldResponse> header = new();
            header.Add(new FieldResponse()
            {
                name = "sumCredit",
                title = "جمع شارژ",
                showInList = true,
                hasFilter = false,
                hasOrder = true,
                showInExtra = false,
                type = "price",
                order = 0,
                enums = new()
            });

            header.Add(new FieldResponse()
            {
                name = "sumDebit",
                title = "جمع مصرف",
                showInList = true,
                hasFilter = false,
                hasOrder = true,
                showInExtra = false,
                type = "price",
                order = 0,
                enums = new()
            });
            #endregion
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Wallets.AsNoTracking();

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

            var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();

            if (user is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

            switch (user.Role)
            {
                case (int)EnumRole.user:
                    Q = Q.Where(A => A.UserId == user.Id);
                    break;
                case (int)EnumRole.customer:
                    Q = Q.Where(A => A.User.CustomerId == user.CustomerId);
                    break;
            }

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Wallets = await Q.ToListAsync();

            var data = new
            {
                sumCredit = Wallets.Where(A => A.Op == (int)EnumWalletOp.Credit || A.Op == (int)EnumWalletOp.AdminCredit).Sum(A => A.Value),
                sumDebit = Wallets.Where(A => A.Op == (int)EnumWalletOp.Debit || A.Op == (int)EnumWalletOp.Refund).Sum(A => A.Value)
            };

            return new ApiOkResponse(_Message: "SUCCESS", _Data: data, _Meta: null, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(WalletInsertRequest request)
        {
            var Wallet = new Wallet()
            {
                UserId = request.userId,
                DateTime = request.dateTime,
                PreValue = request.preValue,
                Op = request.op,
                Value = request.value,
                RemValue = request.remValue,
                Opkey = request.opkey,

            };


            await _NarijeDBContext.Wallets.AddAsync(Wallet);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Wallet.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(WalletEditRequest request)
        {
            var Wallet = await _NarijeDBContext.Wallets
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (Wallet is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Wallet.UserId = request.userId;
            Wallet.DateTime = request.dateTime;
            Wallet.PreValue = request.preValue;
            Wallet.Op = request.op;
            Wallet.Value = request.value;
            Wallet.RemValue = request.remValue;
            Wallet.Opkey = request.opkey;



            _NarijeDBContext.Wallets.Update(Wallet);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Wallet.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Wallet = await _NarijeDBContext.Wallets
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Wallet is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.Wallets.Remove(Wallet);

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
        public async Task<ApiResponse> CloneAsync(WalletCloneRequest request)
        {
            var Wallet = await _NarijeDBContext.Wallets
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Wallet is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion

        public async Task<ApiResponse> UpdateWalletsAsync(int customerId, int creditId)
        {
            try
            {
                var customer = _NarijeDBContext.Customers.Where(c => c.Id == customerId).FirstOrDefault();
                var persianCalendar = new PersianCalendar();
                var currentDate = DateTime.Now;
                var currentYear = persianCalendar.GetYear(currentDate);
                var currentMonth = persianCalendar.GetMonth(currentDate);
                var choosenCredit = _NarijeDBContext.Credits.Where(c => c.Id == creditId).FirstOrDefault();

                if (customer == null)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "شرکت یافت نشد ");

                }
                if (choosenCredit == null)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "اعتبار یافت نشد ");
                }
                if (choosenCredit.Riched == true)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "این اعتبار پرداخت شده است ");
                }

                if (choosenCredit.CustomerId != customer.Id)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "اعتبار مخصوص این شرکت نیست  ");
                }
                if (choosenCredit.Id == customer.LastCreditId)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "این اعتبار پرداخت شده است ");
                }

                if (choosenCredit.Year < currentYear || (choosenCredit.Year < currentYear && choosenCredit.Month < currentMonth))
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "تاریخ این اعتبار گذشته است");

                }
                var isThisSoonPayment = choosenCredit.Month > currentMonth;

                var usersWithLastWallet = _NarijeDBContext.Users
                    .Where(u => u.CustomerId == customer.Id && u.Active == true)
                     .Select(u => new
                     {
                         Id = u.Id,
                         LastWallet = u.Wallets
                             .OrderByDescending(w => w.DateTime)
                                .Select(w => new
                                {
                                    w.Id,
                                    w.DateTime,
                                    w.PreValue,
                                    w.Value,
                                    w.RemValue,
                                    w.Op,
                                    w.Opkey,
                                    w.LastCredit,
                                    w.LastCreditId
                                }).FirstOrDefault(),
                         addCreditToPrevCredit = u.Customer.AddCreditToPrevCredit,

                     })
                    .ToList();




                bool isPayedAny = false;
                var msg = $"شارژ اعتبار ماه {choosenCredit.Month}سال{choosenCredit.Year}";
                using (var transaction = await _NarijeDBContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var user in usersWithLastWallet)
                        {

                            if (user.LastWallet == null)
                            {
                                // Create new wallet if not exists
                                Wallet wallet = new Wallet()
                                {
                                    UserId = user.Id,
                                    DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                    PreValue = 0,
                                    Op = (int)EnumWalletOp.SystemCredit,
                                    Value = choosenCredit.Value,
                                    RemValue = choosenCredit.Value,
                                    LastCreditId = user.Id,
                                    LastCredit = choosenCredit.DateTime,
                                    Description = msg,
                                };

                                wallet.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(wallet);


                                await _NarijeDBContext.Wallets.AddAsync(wallet);

                                WalletPayment wp2 = new WalletPayment()
                                {
                                    Status = 1,
                                    UserId = user.Id,
                                    DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                    UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                    Op = (int)EnumWalletOp.SystemCredit,
                                    Value = choosenCredit.Value,
                                    Gateway = (int)EnumGateway.Wallet,
                                    Wallet = wallet,
                                    Reason = msg,
                                };
                                isPayedAny = true;
                                await _NarijeDBContext.WalletPayments.AddAsync(wp2);
                            }
                            else if (user.LastWallet.LastCreditId != choosenCredit.Id)
                            {
                                // ابتدا تمام اعتبار کیف پول عودت داده میشود سپس مقدار اعتبار کیف پول شارژ می شود

                                if (user.addCreditToPrevCredit == null || user.addCreditToPrevCredit == false)
                                {
                                    var wallet1 = new Wallet()
                                    {
                                        UserId = user.Id,
                                        DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        PreValue = user.LastWallet.Value,
                                        Op = (int)EnumWalletOp.CreditRevoke,
                                        Value = 0,
                                        RemValue = 0,
                                        LastCredit = choosenCredit.DateTime,
                                        Description = "عودت بابت خالی کردن اعتبار ماه قبل "
                                    };

                                    wallet1.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(wallet1);

                                    await _NarijeDBContext.Wallets.AddAsync(wallet1);


                                    WalletPayment wp = new WalletPayment()
                                    {
                                        Status = 1,
                                        UserId = user.Id,
                                        DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        Op = (int)EnumWalletOp.CreditRevoke,
                                        Value = user.LastWallet.Value,
                                        Gateway = (int)EnumGateway.Wallet,
                                        Wallet = wallet1
                                    };
                                    await _NarijeDBContext.WalletPayments.AddAsync(wp);


                                    var wallet2 = new Wallet()
                                    {
                                        UserId = user.Id,
                                        DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        PreValue = 0,
                                        Op = (int)EnumWalletOp.SystemCredit,
                                        Value = choosenCredit.Value,
                                        RemValue = choosenCredit.Value,
                                        LastCreditId = user.Id,
                                        LastCredit = choosenCredit.DateTime,
                                        Description = msg,
                                    };
                                    wallet2.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(wallet2);


                                    await _NarijeDBContext.Wallets.AddAsync(wallet2);



                                    WalletPayment wp2 = new WalletPayment()
                                    {
                                        Status = 1,
                                        UserId = user.Id,
                                        DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        Op = (int)EnumWalletOp.SystemCredit,
                                        Value = choosenCredit.Value,
                                        Gateway = (int)EnumGateway.Wallet,
                                        Wallet = wallet2,
                                        Reason = msg,
                                    };
                                    await _NarijeDBContext.WalletPayments.AddAsync(wp2);

                                }
                                else
                                {


                                    var wallet3 = new Wallet()
                                    {
                                        UserId = user.Id,
                                        DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        PreValue = user.LastWallet.RemValue,
                                        Op = (int)EnumWalletOp.SystemCredit,
                                        Value = choosenCredit.Value,
                                        RemValue = choosenCredit.Value + user.LastWallet.RemValue,

                                        LastCredit = choosenCredit.DateTime,
                                        Description = msg,
                                    };
                                    wallet3.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(wallet3);


                                    await _NarijeDBContext.Wallets.AddAsync(wallet3);



                                    WalletPayment wp3 = new WalletPayment()
                                    {
                                        Status = 1,
                                        UserId = user.Id,
                                        DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                        Op = (int)EnumWalletOp.SystemCredit,
                                        Value = choosenCredit.Value,
                                        Gateway = (int)EnumGateway.Wallet,
                                        Wallet = wallet3,
                                        Reason = msg,
                                    };
                                    await _NarijeDBContext.WalletPayments.AddAsync(wp3);

                                }


                                isPayedAny = true;
                            }
                        }

                        if (isPayedAny == true)
                        {
                            customer.LastCreditId = choosenCredit.Id;
                            _NarijeDBContext.Customers.Update(customer);
                            choosenCredit.Riched = true;
                            _NarijeDBContext.Credits.Update(choosenCredit);
                        }
                        await _NarijeDBContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return new ApiOkResponse(_Message: "SUCCESS", _Data: null);
                    }
                    catch (Exception ex)

                    {
                        await transaction.RollbackAsync();

                        return new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);

                    }

                }

            }
            catch (Exception ex)

            {

                return new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);

            }

        }
    }
}


