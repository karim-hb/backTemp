using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.User;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Narije.Core.DTOs.Enum;
using System.Security.Claims;
using System.Security.Principal;
using Narije.Core.DTOs.User;
using System.Globalization;
using TM.Core.DTOs.Enum;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Narije.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {

        // ------------------
        // Constructor
        // ------------------
        private readonly LogHistoryHelper _logHistoryHelper;
        public UserRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper, LogHistoryHelper logHistoryHelper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
            _logHistoryHelper = logHistoryHelper;

        }

        private async Task<User> CheckAccess()
        {
            //Check Access
            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;
            int id = Convert.ToInt32(Identity.FindFirst("Id").Value);
            var User = await _NarijeDBContext.Users
                                     .Where(A => A.Id == id && (A.Role == 1 || A.Role == 2 || A.Role == 3))
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
            if (User is null)
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
                var User = await _NarijeDBContext.Users
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<UserResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

                User.wallet = await _NarijeDBContext.Wallets
                                                    .Where(A => A.UserId == User.id)
                                                    .OrderByDescending(A => A.Id)
                                                    .Select(A => A.RemValue)
                                                    .FirstOrDefaultAsync();

                User.accessName = await _NarijeDBContext.AccessProfiles
                                                        .Where(A => A.Id == User.accessId)
                                                        .Select(A => A.Title)
                                                        .FirstOrDefaultAsync();

                if (User.role != (int)EnumRole.superadmin && User.role != (int)EnumRole.supervisor)
                {
                    var mealTypes = await _NarijeDBContext.CompanyMeal
                                                          .Where(c => c.CustomerId == User.customerId)
                                                          .Select(c => new { maxReserveTime = c.MaxReserveTime, mealId = c.MealId, mealTitle = c.Meal.Title, galleryId = c.Meal.GalleryId })
                                                          .AsNoTracking()
                                                          .ToListAsync();

                    var branches = await _NarijeDBContext.Customers
                                                         .Where(c => c.Id == User.customerId)
                                                         .Select(c => new { c.BranchForSaturday, c.BranchForSunday, c.BranchForMonday, c.BranchForTuesday, c.BranchForWednesday, c.BranchForThursday, c.BranchForFriday, c.ParentId })
                                                         .FirstOrDefaultAsync();
                    var customerParent = await _NarijeDBContext.Customers.Where(c => c.Id == branches.ParentId).Select(c => new { title = c.Title }).FirstOrDefaultAsync();
                    var tehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
                    var tehranTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tehranTimeZone);

                    var persianCalendar = new System.Globalization.PersianCalendar();
                    var shamsiDayOfWeek = persianCalendar.GetDayOfWeek(tehranTime);

                    object selectedBranch;

                    switch (shamsiDayOfWeek)
                    {
                        case DayOfWeek.Saturday:
                            selectedBranch = branches.BranchForSaturday ?? 0;
                            break;
                        case DayOfWeek.Sunday:
                            selectedBranch = branches.BranchForSunday ?? 0;
                            break;
                        case DayOfWeek.Monday:
                            selectedBranch = branches.BranchForMonday ?? 0;
                            break;
                        case DayOfWeek.Tuesday:
                            selectedBranch = branches.BranchForTuesday ?? 0;
                            break;
                        case DayOfWeek.Wednesday:
                            selectedBranch = branches.BranchForWednesday ?? 0;
                            break;
                        case DayOfWeek.Thursday:
                            selectedBranch = branches.BranchForThursday ?? 0;
                            break;
                        case DayOfWeek.Friday:
                            selectedBranch = branches.BranchForFriday ?? 0;
                            break;

                        default:
                            selectedBranch = branches.BranchForFriday ?? 0;
                            break;
                    }

                    return new ApiOkResponse(_Message: "SUCCEED", _Data: new { User, selectedBranch, mealTypes, customerParent });
                }
                else
                {
                    return new ApiOkResponse(_Message: "SUCCEED", _Data: User);
                }
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.Message);
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

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "User");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.Users
                            .ProjectTo<UserResponse>(_IMapper.ConfigurationProvider);

                var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if ((Identity is null) || (Identity.Claims.Count() == 0))
                    return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

                var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();

                if (user is null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

                switch (user.Role)
                {
                    case (int)EnumRole.user:
                        Q = Q.Where(A => A.id == user.Id);
                        break;
                    case (int)EnumRole.customer:
                        Q = Q.Where(A => A.customerId == user.CustomerId);
                        break;
                }

                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                var Users = await Q.GetPaged(Page: page.Value, Limit: limit.Value);


                var ids = Users.Data.Select(A => A.id);
                var wallets = await _NarijeDBContext.Wallets.Where(A => ids.Contains(A.UserId)).AsNoTracking().ToListAsync();

                foreach (var userD in Users.Data)
                {
                    userD.wallet = wallets.Where(A => A.UserId == userD.id).OrderByDescending(A => A.Id).Select(A => A.RemValue).FirstOrDefault();

                }



                return new ApiOkResponse(_Message: "SUCCESS", _Data: Users.Data, _Meta: Users.Meta, _Header: header);

            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.Message);
            }

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(UserInsertRequest request)
        {
            try
            {
                if (_NarijeDBContext == null)
                    throw new Exception("_NarijeDBContext is null");

                if (request == null)
                    throw new Exception("request is null");

                var mobile = await _NarijeDBContext.Users
                                                   .Where(A => A.Mobile.Equals(request.mobile))
                                                   .AsNoTracking()
                                                   .AnyAsync();

                if (mobile)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "شماره همراه تکراری است");

                var User = new User()
                {
                    Fname = request.fname,
                    Lname = request.lname,
                    Description = request.description,
                    Mobile = request.mobile,
                    CustomerId = request.customerId,
                    Password = BCrypt.Net.BCrypt.HashPassword(request.password),
                    Active = request.active ?? false,
                    Gender = request.gender,
                    Role = request.role ?? 0
                };
                if (request.accessId.HasValue)
                {
                    User.AccessId = request.accessId;
                }
                if (request.fromGallery != null) User.GalleryId = await GalleryHelper.AddFromGallery(_NarijeDBContext, request.fromGallery);
                if (request.files != null)
                {
                    var k = await GalleryHelper.AddToGallery(_NarijeDBContext, "User", request.files.FirstOrDefault());
                    if (k > 0)
                        User.GalleryId = k;
                }

                if (_IHttpContextAccessor == null || _IHttpContextAccessor.HttpContext == null)
                    throw new Exception("_IHttpContextAccessor or _IHttpContextAccessor.HttpContext is null");

                var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if (Identity == null || !Identity.Claims.Any())
                    return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

                var userIdClaim = Identity.FindFirst("Id");
                if (userIdClaim == null)
                    throw new Exception("User Id claim is null");

                var user = await _NarijeDBContext.Users
                                                 .Where(A => A.Id == Int32.Parse(userIdClaim.Value))
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync();

                if (user == null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");


                await _NarijeDBContext.Users.AddAsync(User);

                // افزودن اعتبار ماهانه شرکت به کاربر جدید و تشکیل کیف پول کاربر

                if (request.customerId.HasValue)

                {

                    var customer = _NarijeDBContext.Customers.Where(c => c.Id == request.customerId).FirstOrDefault();

                    if (customer != null & customer.PayType == 1)
                    {


                        var lastCredit = _NarijeDBContext.Credits.Where(c => c.Id == customer.LastCreditId).FirstOrDefault();


                        var persianCalendar = new PersianCalendar();
                        var currentDate = DateTime.Now;
                        var currentYear = persianCalendar.GetYear(currentDate);
                        var currentMonth = persianCalendar.GetMonth(currentDate);



                        if (lastCredit != null)
                        {
                            var msg = $"شارژ اعتبار ماه {lastCredit.Month}سال{lastCredit.Year}";
                            var totalCreditValue = lastCredit.Value;
                            var latestCreditDate = lastCredit.DateTime;
                            var wallet = new Wallet()
                            {
                                User = User,
                                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                PreValue = 0,
                                Op = (int)EnumWalletOp.SystemCredit,
                                Value = totalCreditValue,
                                RemValue = totalCreditValue,
                                LastCreditId = lastCredit.Id,
                                LastCredit = latestCreditDate,
                                Description = msg,
                            };

                            wallet.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(wallet);


                            await _NarijeDBContext.Wallets.AddAsync(wallet);

                            WalletPayment wp2 = new WalletPayment()
                            {
                                Status = 1,
                                User = User,
                                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                                Op = (int)EnumWalletOp.SystemCredit,
                                Value = totalCreditValue,
                                Gateway = (int)EnumGateway.Wallet,
                                Wallet = wallet,
                                Reason = msg,

                            };
                            await _NarijeDBContext.WalletPayments.AddAsync(wp2);
                        }

                    }
                }


                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

                return await GetAsync(User.Id);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(UserEditRequest request)
        {

            var User = await _NarijeDBContext.Users
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (User is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            User.Fname = request.fname;
            User.Lname = request.lname;
            User.Description = request.description;
            User.Gender = request.gender;


            User.Mobile = request.mobile;
            if (request.password != null)
                User.Password = BCrypt.Net.BCrypt.HashPassword(request.password);
            if (request.active != null)
                User.Active = request.active.Value;
            User.GalleryId = await GalleryHelper.EditFromGallery(_NarijeDBContext, User.GalleryId, request.fromGallery);
            if (request.files != null)
                User.GalleryId = await GalleryHelper.EditGallery(_NarijeDBContext, User.GalleryId, "User", request.files.FirstOrDefault());


            User.Role = request.role ?? 0;
            if (request.role == (int)EnumRole.user)
            {
                User.AccessId = null;

            }
            else
            {
                User.AccessId = request.accessId;
            }

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

            var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();

            if (user is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

            if (User.CustomerId != request.customerId)
            {
                var customer = _NarijeDBContext.Customers.Where(c => c.Id == User.CustomerId).FirstOrDefault();
                if (customer.PayType == 1)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "  ویرایش شرکت های اعتباری در حال حاظر امکان پذیر نیست");
                }
                User.CustomerId = request.customerId;
                var tomorrow = DateTime.Today.AddDays(1);

                var futureReserves = await _NarijeDBContext.Reserves
                    .Where(r => r.UserId == request.id && r.DateTime >= tomorrow)
                    .ToListAsync();

                if (futureReserves.Any())
                {
                    _NarijeDBContext.Reserves.RemoveRange(futureReserves);
                }

            }

            _NarijeDBContext.Users.Update(User);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(User.Id);
        }
        #endregion


        #region EditActiveAsync
        // ------------------
        //  EditActiveAsync
        // ------------------
        public async Task<ApiResponse> EditActiveAsync(int id)
        {
            try
            {
                var Data = await _NarijeDBContext.Users.FirstOrDefaultAsync(A => A.Id == id);
                if (Data is null)
                    return new ApiErrorResponse(StatusCodes.Status404NotFound, "اطلاعات جهت ویرایش یافت نشد");

                Data.Active = !Data.Active;
                _NarijeDBContext.Users.Update(Data);


                await _NarijeDBContext.SaveChangesAsync();

                return await GetAsync(Data.Id);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(StatusCodes.Status400BadRequest, ex.Message);
            }
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var User = await _NarijeDBContext.Users
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (User is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            try
            {
                _NarijeDBContext.Users.Remove(User);

                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            }
            catch (Exception ex)
            {
                if ((ex.InnerException != null) && (((Microsoft.Data.SqlClient.SqlException)ex.InnerException).Number == 547))
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات در حال استفاده قابل حذف نیست");
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات حذف نشد! دوباره سعی کنید");
            }

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(UserCloneRequest request)
        {
            var User = await _NarijeDBContext.Users
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (User is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion

        #region UserPermissionsAsync
        /// <summary>
        /// دسترسی های کاربر
        /// </summary>
        public async Task<ApiResponse> UserPermissionsAsync()
        {
            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

            List<string> result = new();


            var Admin = await _NarijeDBContext.Users.Where(A => A.Id == Convert.ToInt32(Identity.FindFirst("Id").Value)).FirstOrDefaultAsync();

            if (Admin is null)
                return new ApiOkResponse(_Message: "SUCCESS", _Data: result);


            if (Admin.Role == (int)EnumRole.superadmin)
            {
                var res = await _NarijeDBContext.Premissions
                                        .Select(A => A.Value)
                                        .ToListAsync();

                return new ApiOkResponse(_Message: "SUCCESS", _Data: res);
            }


            var access = await _NarijeDBContext.AccessProfiles
                                    .Where(A => A.Id == Admin.AccessId)
                                    .FirstOrDefaultAsync();

            if (access is null)
                return new ApiOkResponse(_Message: "SUCCESS", _Data: result);

            var permissions = await _NarijeDBContext.AccessPermissions
                                    .Where(A => A.AccessId == access.Id)
                                    .Select(A => A.PermissionId)
                                    .ToListAsync();

            if (permissions.Count() == 0)
                return new ApiOkResponse(_Message: "SUCCESS", _Data: result);

            result = await _NarijeDBContext.Premissions
                                    .Where(A => permissions.Contains(A.Id))
                                    .Select(A => A.Value)
                                    .ToListAsync();


            return new ApiOkResponse(_Message: "SUCCESS", _Data: result);
        }
        #endregion

        #region ChangePasswordAsync
        // ------------------
        //  ChangePasswordAsync
        // ------------------
        public async Task<ApiResponse> ChangePasswordAsync(UserChangePasswordRequest request)
        {
            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");

            var User = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).FirstOrDefaultAsync();

            if (User is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

            User.Password = BCrypt.Net.BCrypt.HashPassword(request.newPassword);

            _NarijeDBContext.Users.Update(User);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(User.Id);
        }
        #endregion

        #region InsertFromExel
        public async Task<ApiResponse> ProcessUserFileAsync(IFormFile file)
        {
            try
            {
                var userUpdates = new List<UserEditRequest>();
                var newUsers = new List<User>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    if (file.FileName.EndsWith(".csv"))
                    {
                        using (var reader = new StreamReader(stream))
                        using (var csv = new CsvHelper.CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                        {
                            var records = csv.GetRecords<UserEditRequest>().ToList();
                            userUpdates.AddRange(await ProcessUserRecordsAsync(records, newUsers));
                        }
                    }
                    else if (file.FileName.EndsWith(".xlsx"))
                    {
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        using (var package = new ExcelPackage(stream))
                        {
                            var worksheet = package.Workbook.Worksheets[0];
                            var records = new List<UserEditRequest>();
                            int totalRows = worksheet.Dimension.End.Row - 1; // minus 1 for header row

                            var headers = worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column]
                                .ToDictionary(cell => cell.Text.Trim(), cell => cell.Start.Column);

                            var mobileNumbers = new HashSet<string>();
                            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                            {
                                var mobile = worksheet.Cells[row, headers["شماره همراه"]].Text.Trim();
                                if (string.IsNullOrEmpty(mobile))
                                {
                                    return new ApiErrorResponse(
                                        _Code: StatusCodes.Status400BadRequest,
                                        _Message: $"خطا: شماره همراه در ردیف {row} خالی است."
                                    );
                                }

                                if (mobileNumbers.Contains(mobile))
                                {
                                    return new ApiErrorResponse(
                                        _Code: StatusCodes.Status400BadRequest,
                                        _Message: $"خطا: شماره همراه تکراری '{mobile}' در ردیف {row} یافت شد."
                                    );
                                }

                                mobileNumbers.Add(mobile);

                                records.Add(new UserEditRequest
                                {
                                    id = int.TryParse(worksheet.Cells[row, headers["ایدی کاربری"]].Text, out var id) ? id : 0,
                                    fname = worksheet.Cells[row, headers["نام"]].Text,
                                    lname = worksheet.Cells[row, headers["نام خانوادگی"]].Text,
                                    mobile = mobile,
                                    description = worksheet.Cells[row, headers["توضیحات"]].Text,
                                    role = MapRoleToEnum(worksheet.Cells[row, headers["نقش"]].Text),
                                    customerId = int.TryParse(worksheet.Cells[row, headers["ایدی شعبه"]].Text, out var accessId) ? accessId : (int?)null,
                                    active = ParseBoolean(worksheet.Cells[row, headers["فعال/غیرفعال"]].Text),
                                    gender = ParseGender(worksheet.Cells[row, headers["جنسیت"]].Text) ?? true,
                                    password = mobile // Default password is the phone number
                                });
                            }

                            userUpdates.AddRange(await ProcessUserRecordsAsync(records, newUsers));
                        }
                    }
                }

                await _NarijeDBContext.SaveChangesAsync();

                foreach (var newUser in newUsers)
                {
                    await _logHistoryHelper.AddLogHistoryAsync(
                        "User",
                        newUser.Id,
                        EnumLogHistroyAction.create,
                        EnumLogHistorySource.excel,
                        JsonSerializer.Serialize(newUser),
                        true
                    );
                }

                return new ApiOkResponse("فایل با موفقیت پردازش شد.");
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status400BadRequest,
                    _Message: $"خطا در پردازش فایل: {ex.Message}"
                );
            }
        }
        private async Task<List<UserEditRequest>> ProcessUserRecordsAsync(IEnumerable<UserEditRequest> records, List<User> newUsers)

        {
            var userUpdates = new List<UserEditRequest>();

            foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record.mobile))
                {
                    Console.WriteLine("Skipping record: Mobile number is required.");
                    continue;
                }

                var existingUser = await _NarijeDBContext.Users
                    .Where(u => u.Id == record.id || u.Mobile == record.mobile)
                    .FirstOrDefaultAsync();



                if (existingUser == null)
                {
                    var newUser = new User
                    {
                        Fname = record.fname,
                        Lname = record.lname,
                        Mobile = record.mobile,
                        CustomerId = record.customerId,
                        Description = record.description,
                        Role = record.role ?? 0,
                        Active = record.active ?? true,
                        Password = BCrypt.Net.BCrypt.HashPassword(record.mobile),
                        Gender = record.gender
                    };


                    _NarijeDBContext.Users.Add(newUser);

                    newUsers.Add(newUser);
                }
                else
                {
                    existingUser.Fname = record.fname;
                    existingUser.Lname = record.lname;
                    existingUser.Description = record.description;
                    existingUser.Active = record.active ?? existingUser.Active;
                    existingUser.Gender = record.gender;
                    var changes = LogHistoryHelper.GetEntityChanges(record, existingUser);

                    if (changes.Count > 0)
                    {
                        await _logHistoryHelper.AddLogHistoryAsync(
                                  "User",
                                     existingUser.Id,
                                 EnumLogHistroyAction.update,
                                 EnumLogHistorySource.excel,
                                    JsonSerializer.Serialize(record),
                                false
                            );
                    }

                    _NarijeDBContext.Users.Update(existingUser);
                }

            }

            return userUpdates;
        }

        private int? MapRoleToEnum(string roleName)
        {
            return roleName switch
            {
                "کارمند" => (int)EnumRole.user,
                "ادمین شرکت" => (int)EnumRole.customer,
                "ادمین سایت" => (int)EnumRole.supervisor,
                _ => null
            };
        }

        private bool? ParseBoolean(string text)
        {
            return text?.Trim() switch
            {
                "بلی" => true,
                "خیر" => false,
                _ => null
            };
        }
        private bool? ParseGender(string text)
        {
            return text?.Trim() switch
            {
                "مرد" => true,
                "زن" => false,
                _ => null
            };
        }
        #endregion
    }
}


