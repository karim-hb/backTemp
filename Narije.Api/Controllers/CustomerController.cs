using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using mpNuget;
using Narije.Core.DTOs.Admin;
using Newtonsoft.Json;
using ServiceReference1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Narije.Api.Helpers;
using Narije.Core.DTOs.Public;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;
using Narije.Core.DTOs.Enum;
using Narije.Core.DTOs.User;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Narije.Api.Controllers
{
    /// <summary>
    /// Home
    /// </summary>
    [Route("customer")]
    [Authorize(Roles = "customer")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly NarijeDBContext _NarijeDBContext;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IConfiguration _IConfiguration;
        private readonly IHttpClientFactory _IHttpClientFactory;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        private static string BucketServiceURL = "https://tahlilmobile-gallery.storage.iran.liara.space/";

        /// <summary>
        /// متد سازنده
        /// </summary>
        public CustomerController(NarijeDBContext NarijeDBContext, IHttpContextAccessor iHttpContextAccessor, IConfiguration iConfiguration, IHttpClientFactory iHttpClientFactory, IWebHostEnvironment iWebHostEnvironment)
        {
            _NarijeDBContext = NarijeDBContext;
            _IHttpContextAccessor = iHttpContextAccessor;
            _IConfiguration = iConfiguration;
            _IHttpClientFactory = iHttpClientFactory;
            _IWebHostEnvironment = iWebHostEnvironment;

            BucketServiceURL = _IConfiguration.GetSection("baseURL").Value + "/public/downloadFileById/";
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
                                     .Include(A => A.Customer)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
            if (User is null)
                return null;

            return User;
        }

        #region Group
        /// <summary>
        /// لیست گروه ها
        /// </summary>
        [Route("groups")]
        [HttpGet]
        public async Task<IActionResult> Groups([FromQuery] int? Page, [FromQuery] int? Limit, [FromQuery] bool? isFood)
        {
            try
            {
                if ((Page is null) || (Page == 0))
                    Page = 1;
                if ((Limit is null) || (Limit == 0) || (Limit > 1000))
                    Limit = 30;

                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.FoodGroups
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
                                            image = A.GalleryId
                                        })
                                        .AsNoTracking();

           
                var data = await Q.GetPaged(Page: Page.Value, Limit: Limit.Value);

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data.Data, _Meta: data.Meta));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #region Customer -------------------------------------------------------------
        /// <summary>
        /// اطلاعات مشتری 
        /// </summary>
        [Route("customer")]
        [HttpGet]
        public async Task<IActionResult> Customer()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = _NarijeDBContext.Customers
                                        .Where(A => A.Id == Admin.CustomerId)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
                                            tel = A.Tel,
                                            contractStartDate = A.ContractStartDate,
                                            address = A.Address,
                                            //cancelPercent = A.CancelPercent,
                                            //cancelPercentPeriod = A.CancelPercentPeriod,
                                            //cancelTime = A.CancelTime,
                                            guestTime = A.GuestTime,
                                            reserveTime = A.ReserveTime,
                                            //reserveAfter = A.ReserveAfter,
                                            reserveTo = A.ReserveTo,
                                            active = A.Active,
                                            foodType = A.FoodType,
                                            showPrice = A.ShowPrice,
                                            minReserve = A.MinReserve
                                        })
                                        .AsNoTracking();

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region Users -------------------------------------------------------------OK
        /// <summary>
        /// شمارش کاربران
        /// </summary>
        [Route("usersCount")]
        [HttpGet]
        public async Task<IActionResult> UsersCount()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Users
                                         .Where(A => A.CustomerId == Admin.CustomerId)  
                                         .AsNoTracking()
                                         .CountAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: data));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// لیست کاربران
        /// مرتب سازی
        /// 1 آی دی
        /// 2 نام
        /// 3 نام خانوادگی
        /// </summary>
        [Route("users")]
        [HttpGet]
        public async Task<IActionResult> Users([FromQuery] int? Page, int? Limit, string search, int? role, string order)
        {
            try
            {
                if ((Page is null) || (Page == 0))
                    Page = 1;
                if ((Limit is null) || (Limit == 0) || (Limit > 1000))
                    Limit = 30;

                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Users
                                         .Where(A => A.CustomerId == Admin.CustomerId)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             fName = A.Fname,
                                             lName = A.Lname,
                                             description = A.Description,
                                             lastLogin = A.LastLogin,
                                             mobile = A.Mobile,
                                             role = A.Role,
                                             active = A.Active
                                         });

                if (order != null)
                {
                    switch (order)
                    {
                        case "1":
                            Q = Q.OrderBy(A => A.id);
                            break;
                        case "2":
                            Q = Q.OrderBy(A => A.fName);
                            break;
                        case "3":
                            Q = Q.OrderBy(A => A.lName);
                            break;
                    }
                }

                if (search != null)
                    Q = Q.Where(A => A.fName.Contains(search) || A.lName.Contains(search) || A.mobile.Contains(search));

                if (role != null)
                    Q = Q.Where(A => A.role == role);

                var data = await Q.GetPaged(Page: Page.Value, Limit: Limit.Value);

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data.Data, _Meta: data.Meta));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// اطلاعات کاربر
        /// </summary>
        [Route("user")]
        [HttpGet]
        public async Task<IActionResult> User([FromQuery] int id)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Users
                                         .Where(A => A.Id == id && A.CustomerId == Admin.CustomerId)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             fName = A.Fname,
                                             lName = A.Lname,
                                             description = A.Description,
                                             lastLogin = A.LastLogin,
                                             mobile = A.Mobile,
                                             role = A.Role,
                                             active = A.Active
                                         })
                                         .FirstOrDefaultAsync();


                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ویرایش کاربران
        /// </summary>
        [Route("user")]
        [HttpPut]
        public async Task<IActionResult> EditUser([FromBody] UserRequest user)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Users.Where(A => A.Id == user.id && A.CustomerId == Admin.CustomerId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "کاربر یافت نشد"));

                var mobile = _NarijeDBContext.Users
                                    .Where(A => A.Mobile.Equals(user.mobile) && A.Id != user.id)
                                    .AsNoTracking()
                                    .FirstOrDefault();

                if (mobile != null)
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "شماره موبایل تکراری است"));

                data.Fname = user.fName;
                data.Lname = user.lName;
                data.Mobile = user.mobile;
                data.CustomerId = Admin.CustomerId;
                if (user.active != null)
                    data.Active = user.active.Value;
                if (user.role != null)
                    if (user.role != (int)EnumRole.supervisor)
                        data.Role = user.role.Value;

                _NarijeDBContext.Users.Update(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    fName = data.Fname,
                    lName = data.Lname,
                    description = data.Description,
                    mobile = data.Mobile,
                    role = data.Role,
                    active = data.Active
                };

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: result));

            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// افزودن کاربران
        /// </summary>
        [Route("user")]
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserRequest user)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));


                var mobile = _NarijeDBContext.Users
                                    .Where(A => A.Mobile.Equals(user.mobile))
                                    .AsNoTracking()
                                    .FirstOrDefault();

                if (mobile != null)
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "شماره موبایل تکراری است"));

                var data = new User();

                data.Fname = user.fName;
                data.Lname = user.lName;
                data.Description = user.description;
                data.Password = BCrypt.Net.BCrypt.HashPassword(user.password);
                data.Mobile = user.mobile;
                data.CustomerId = Admin.CustomerId;
                if (user.active != null)
                    data.Active = user.active.Value;

                await _NarijeDBContext.Users.AddAsync(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    fName = data.Fname,
                    lName = data.Lname,
                    description = data.Description,
                    mobile = data.Mobile,
                    role = data.Role,
                    active = data.Active
                };

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: result));

            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// حذف کاربر
        /// </summary>
        [Route("user/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                if (id == Admin.Id)
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "اکانت کاربری خود را نمی توانید حذف کنید"));

                var data = await _NarijeDBContext.Users.Where(A => A.Id == id && A.CustomerId == Admin.CustomerId)
                                         .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "کاربر یافت نشد"));

                _NarijeDBContext.Users.Remove(data);
                await _NarijeDBContext.SaveChangesAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCEED"));
            }
            catch (Exception Ex)
            {
                if (Ex.Source.Equals("Microsoft.EntityFrameworkCore.Relational"))
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new { message = "خطا در پاک کردن کاربر" });
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// تغییر رمز کاربر
        /// </summary>
        [Route("changeUserPassword")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordRequest info)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var User = await _NarijeDBContext.Users
                                        .Where(A => A.Id == info.id && A.CustomerId == Admin.CustomerId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (User is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "کاربر یافت نشد"));

                User.Password = BCrypt.Net.BCrypt.HashPassword(info.password);

                _NarijeDBContext.Users.Update(User);
                await _NarijeDBContext.SaveChangesAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCEED"));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region Menu -------------------------------------------------------------
        /// <summary>
        /// اطلاعات منوی غذایی
        /// </summary>
        [Route("menu")]
        [HttpGet]
        public async Task<IActionResult> Menu([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Menus
                                        .Where(A => A.CustomerId == Admin.CustomerId)
                                        .Select(A => A.DateTime)
                                        .Distinct();

                if (fromDate != null)
                    Q = Q.Where(A => A.Date >= fromDate.Value.Date);
                if (toDate != null)
                    Q = Q.Where(A => A.Date <= toDate.Value.Date);

                if(await Q.CountAsync() == 0)
                {
                    Q = _NarijeDBContext.Menus
                                            .Where(A => A.CustomerId == Admin.Customer.ParentId)
                                            .Select(A => A.DateTime)
                                            .Distinct();

                    if (fromDate != null)
                        Q = Q.Where(A => A.Date >= fromDate.Value.Date);
                    if (toDate != null)
                        Q = Q.Where(A => A.Date <= toDate.Value.Date);

                }

                var menus = await Q.Select(A => new MenuDayResponse()
                {
                    datetime = A
                }).ToListAsync();

                foreach (var item in menus)
                {
                    item.foods = await _NarijeDBContext.Menus
                                        .Where(A => A.CustomerId == Admin.CustomerId && A.DateTime.Date == item.datetime)
                                        .Select(A => new MenuFoodResponse()
                                        {
                                            maxReserve = A.MaxReserve,
                                            foodId = A.FoodId,
                                            food = A.Food.Title,
                                            foodDescription = A.Food.Description,
                                            foodGroupId = A.Food.GroupId,
                                            foodGroup = A.Food.Group.Title
                                        })
                                        .ToListAsync();
                }


                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: menus));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region UserReserve -------------------------------------------------------------
        /// <summary>
        /// مشاهده رزور کارمند
        /// week 0 هفته جاری
        /// </summary>
        [Route("userReserves")]
        [HttpGet]
        public async Task<IActionResult> UserReserves([FromQuery] DateTime fromDate, DateTime toDate, int userId, string search, int? groupId , int mealId, int month, int year)
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var exists = await _NarijeDBContext.Users
                                    .Where(A => A.Id == userId && A.CustomerId == User.CustomerId)
                                    .FirstOrDefaultAsync();
                if (exists is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "کاربر یافت نشد"));


                //DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                //int diff = (7 + (today.DayOfWeek - DayOfWeek.Saturday)) % 7;
                //var StartWeek = today.AddDays(-1 * diff).Date;
                //var EndWeek = StartWeek.AddDays(6).Date;

                var StartWeek = fromDate;
                var EndWeek = toDate;

                ReserveHelper helper = new();
                var reserves = await helper.GetUserReserve(_NarijeDBContext, exists, StartWeek, EndWeek, search, groupId, BucketServiceURL, 0, mealId, month, year);///-1

                /*
                var UserReserves = await _NarijeDBContext.Reserves
                                    .Where(A => A.UserId == userId && A.DateTime.Date >= StartWeek.Date && A.DateTime.Date <= EndWeek.Date)
                                    .AsNoTracking()
                                    .ToListAsync();
                
                int ftype = (int)EnumFoodType.special;
                if (User.Customer != null)
                    ftype = User.Customer.FoodType;


                List<ReservesResponse> reserves = new();
                while (StartWeek <= EndWeek)
                {
                    var Q = _NarijeDBContext.Menus
                                        .Where(A => A.CustomerId == User.CustomerId && A.DateTime.Date == StartWeek.Date)
                                        .Select(A => new ReserveResponse()
                                        {
                                            id = A.Id,
                                            maxReserve = A.MaxReserve,
                                            foodId = A.FoodId,
                                            food = A.Food.Title,
                                            foodGroupId = A.Food.GroupId,
                                            foodGroup = A.Food.Group.Title,
                                            image = A.Food.Gallery == null ? "" : $"{BucketServiceURL}{A.Food.GalleryId}",
                                            state = "",
                                            price = (ftype == (int)EnumFoodType.echo) ? A.Food.FoodPrices.Where(A => A.CustomerId == User.CustomerId).Select(A => A.EchoPrice).FirstOrDefault() :
                                                            A.Food.FoodPrices.Where(A => A.CustomerId == User.CustomerId).Select(A => A.SpecialPrice).FirstOrDefault(),
                                            qty = 0
                                        });
                    if (groupId != null)
                        Q = Q.Where(A => A.foodGroupId == groupId);
                    if (search != null)
                        Q = Q.Where(A => A.food.Contains(search));

                    var items = await Q.ToListAsync();

                    foreach (var item in items)
                    {
                        var r = UserReserves
                                    .Where(A => A.DateTime.Date == StartWeek.Date && A.FoodId == item.foodId)
                                    .FirstOrDefault();
                        if (r != null)
                        {
                            item.state = EnumHelper<EnumReserveState>.GetDisplayValue((EnumReserveState)r.State);
                            item.qty = r.Num;
                            item.price = r.Price;
                        }
                    }

                    reserves.Add(new ReservesResponse()
                    {
                        datetime = StartWeek,
                        reserves = items
                    });

                    StartWeek = StartWeek.AddDays(1).Date;
                }
                */

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: reserves));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ویرایش رزرو کارمند
        /// </summary>
        [Route("userReserves/{userId}")]
        [HttpPut]
        public async Task<IActionResult> EditUserReserves([FromRoute]int userId, [FromBody] ReservesRequest days)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var User = await _NarijeDBContext.Users
                                    .Where(A => A.Id == userId && A.CustomerId == Admin.CustomerId)
                                    .Include(A => A.Customer)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
                if (User is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "کاربر یافت نشد"));

                if(User.Active == false)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "پرسنل غیر فعال است"));

                ReserveHelper helper = new();

                var result = await helper.Reserve(_NarijeDBContext, User, days, (int)EnumReserveState.normal, true);

                if (result == "")
                    return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: null));
                else
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #region CustomerReserve -------------------------------------------------------------
        /// <summary>
        /// مشاهده رزور گروهی
        /// week 0 هفته جاری
        /// </summary>
        [Route("customerReserves")]
        [HttpGet]
        public async Task<IActionResult> CustomerReserves([FromQuery] DateTime fromDate, DateTime toDate, string search, int? groupId,int mealId, int month, int year)
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var StartWeek = fromDate;
                var EndWeek = toDate;

                ReserveHelper helper = new();
                var reserves = await helper.GetUserReserve(_NarijeDBContext, User, StartWeek, EndWeek, search, groupId, BucketServiceURL, (int)EnumReserveState.normal, mealId, month, year);

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: reserves));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ویرایش رزرو گروهی
        /// </summary>
        [Route("customerReserves")]
        [HttpPut]
        public async Task<IActionResult> EditCustomerReserves([FromBody] ReservesRequest days)
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                ReserveHelper helper = new();

                var result = await helper.Reserve(_NarijeDBContext, User, days, (int)EnumReserveState.normal, true);

                if (result == "")
                    return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: null));
                else
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #region GuestReserve -------------------------------------------------------------
        /// <summary>
        /// مشاهده رزور مهمان
        /// week 0 هفته جاری
        /// </summary>
        [Route("guestReserves")]
        [HttpGet]
        public async Task<IActionResult> GuestReserves([FromQuery] DateTime fromDate, DateTime toDate, string search, int? groupId , int mealId, int month, int year)
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var StartWeek = fromDate;
                var EndWeek = toDate;


                ReserveHelper helper = new();
                var reserves = await helper.GetUserReserve(_NarijeDBContext, User, StartWeek, EndWeek, search, groupId, BucketServiceURL, (int)EnumReserveState.guest, mealId, month, year);

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: reserves));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ویرایش رزرو مهمان
        /// </summary>
        [Route("guestReserves")]
        [HttpPut]
        public async Task<IActionResult> EditGuestReserves([FromBody] ReservesRequest days)
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                ReserveHelper helper = new();

                var result = await helper.Reserve(_NarijeDBContext, User, days, (int)EnumReserveState.guest, true);

                if (result == "")
                    return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: null));
                else
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #region Reports
        /// <summary>
        ///  لیست رزرو ها
        ///  ترتیب
        ///  1 بر اساس تاریخ صعودی
        ///  2 بر اساس تاریخ نزولی
        ///  3 بر اساس تعداد صعودی
        ///  4 بر اساس تعداد نزولی
        ///  5 بر اساس نام و نام خانوادگی صعودی
        ///  6 بر اساس نام و نام خانوادگی نزولی
        ///  </summary>
        [Route("reservesReport")]
        [HttpGet]
        public async Task<IActionResult> ReservesReport([FromQuery] int? Page, int? Limit, string order, string search, DateTime? fromDate, DateTime? toDate,
                                                            int? foodId, int? foodGroupId, int? userId, int? state)
        {
            if ((Page is null) || (Page == 0))
                Page = 1;
            if ((Limit is null) || (Limit == 0))
                Limit = 30;
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Reserves
                                         .Where(A => A.CustomerId == Admin.CustomerId)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             state = A.State,
                                             userId = A.UserId,
                                             userName = A.User.Fname + " " + A.User.Lname,
                                             userMobile = A.User.Mobile,
                                             customer = A.Customer.Title,
                                             customerId = A.CustomerId,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             price = A.Price,
                                             foodGroupId = A.Food.GroupId,
                                             foodGroup = A.Food.Group.Title,
                                             isFood = A.Food.IsFood,
                                             qty = A.Num,
                                             foodType = EnumHelper<EnumFoodType>.GetDisplayValue((EnumFoodType)A.FoodType)
                                         });

                if (search is not null)
                    Q = Q.Where(A => A.userName.Contains(search) || A.food.Contains(search) || A.foodGroup.Contains(search));
                if (fromDate is not null)
                    Q = Q.Where(A => A.dateTime.Date >= fromDate.Value.Date);
                if (toDate is not null)
                    Q = Q.Where(A => A.dateTime.Date <= toDate.Value.Date);
                if (foodId is not null)
                    Q = Q.Where(A => A.foodId == foodId);
                if (userId is not null)
                    Q = Q.Where(A => A.userId == userId);
                if (foodGroupId is not null)
                    Q = Q.Where(A => A.foodGroupId == foodGroupId);
                if (state is not null)
                    Q = Q.Where(A => A.state == state);

                if (order is not null)
                {
                    switch (order)
                    {
                        case "1":  //بر اساس تاریخ صعودی
                            Q = Q.OrderBy(A => A.dateTime);
                            break;
                        case "2":  //بر اساس تاریخ نزولی
                            Q = Q.OrderByDescending(A => A.dateTime);
                            break;
                        case "3":  //بر اساس تعداد صعودی
                            Q = Q.OrderBy(A => A.qty);
                            break;
                        case "4":  //بر اساس تعداد نزولی
                            Q = Q.OrderByDescending(A => A.qty);
                            break;
                        case "5":  //صعودی بر اساس نام
                            Q = Q.OrderBy(A => A.userName);
                            break;
                        case "6":  //نزولی بر اساس نام
                            Q = Q.OrderByDescending(A => A.userName);
                            break;
                    }
                }
                else
                    Q = Q.OrderByDescending(A => A.id);


                var data = await Q.GetPaged(Page: Page.Value, Limit: Limit.Value);

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data.Data, _Meta: data.Meta));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #region Export ---------------------------------------------------------------
        /// <summary>
        ///  لیست رزرو ها
        ///  ترتیب
        ///  1 بر اساس تاریخ صعودی
        ///  2 بر اساس تاریخ نزولی
        ///  3 بر اساس تعداد صعودی
        ///  4 بر اساس تعداد نزولی
        ///  5 بر اساس نام و نام خانوادگی صعودی
        ///  6 بر اساس نام و نام خانوادگی نزولی
        ///  </summary>
        [Route("exportReserves")]
        [HttpGet]
        public async Task<IActionResult> ExportReserves([FromQuery] List<int> ids)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Reserves
                                         .Where(A => A.CustomerId == Admin.CustomerId)
                                         .Select(A => new ExportResservesResponse()
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date.ToString(),
                                             userId = A.UserId,
                                             userName = A.User.Fname + " " + A.User.Lname,
                                             userMobile = A.User.Mobile,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             foodGroup = A.Food.Group.Title,
                                             price = A.Price,
                                             qty = A.Num,
                                             foodType = EnumHelper<EnumFoodType>.GetDisplayValue((EnumFoodType)A.FoodType)
                                         });

                if ((ids != null) && ids.Count > 0)
                {
                    Q = Q.Where(A => ids.Contains(A.id));
                }

                var data = await Q.ToListAsync();

                foreach(var item in data)
                {
                    item.dateTime = PersianDateTimeUtils.ToPersianDateTimeString(DateTime.Parse(item.dateTime), "yyyy-MM-dd");
                }

                var body = data.Select(A => new List<string>
                {
                    A.userId.ToString(),
                    A.dateTime.ToString(),
                    A.userName,
                    A.userMobile,
                    A.food,
                    A.foodGroup,
                    A.price.ToString(),
                    A.foodType,
                    A.qty.ToString()
                });

                var result = new
                {
                    header = new List<string>
                    {
                        "آی دی",
                        "تاریخ",
                        "نام کارمند",
                        "موبایل کارمند",
                        "نام غذا",
                        "گروه غذا",
                        "قیمت",
                        "حجم غذا",
                        "تعداد"
                    },
                    body = body
                };

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        ///  خروجی شماره 2
        /// </summary>
        [Route("exportUsers")]
        [HttpGet]
        public async Task<IActionResult> ExportUsers([FromQuery] List<int> ids)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Users
                                         .Where(A => A.CustomerId == Admin.CustomerId)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             fName = A.Fname,
                                             lName = A.Lname,
                                             description = A.Description,
                                             lastLogin = A.LastLogin.ToString(),
                                             mobile = A.Mobile,
                                             role = EnumHelper<EnumRole>.GetDisplayValue((EnumRole)A.Role)
                                         });

                if ((ids != null) && ids.Count > 0)
                {
                    Q = Q.Where(A => ids.Contains(A.id));
                }

                var data = await Q.ToListAsync();

                var body = data.Select(A => new List<string>
                {
                    A.id.ToString(),
                    A.fName,
                    A.lName,
                    A.mobile,
                    A.lastLogin,
                    A.role
                });

                var result = new
                {
                    header = new List<string>
                    {
                        "آی دی",
                        "نام",
                        "نام خانوادگی",
                        "موبایل",
                        "آخرین لاگین",
                        "نقش"
                    },
                    body = body
                };

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion


    }
}