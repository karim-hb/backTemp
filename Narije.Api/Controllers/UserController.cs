using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using mpNuget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;
using Narije.Core.DTOs.Public;
using Narije.Api.Helpers;
using Narije.Core.DTOs.Admin;
using Narije.Core.DTOs.User;
using Narije.Core.DTOs.Enum;
using Microsoft.AspNetCore.Authentication;

namespace Narije.Api.Controllers
{
    /// <summary>
    /// Home
    /// </summary>
    [Route("user")]
    [Authorize(Roles = "user")]
    [ApiController]
    public class UserController : ControllerBase
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
        public UserController(NarijeDBContext NarijeDBContext, IHttpContextAccessor iHttpContextAccessor, IConfiguration iConfiguration, IHttpClientFactory iHttpClientFactory, IWebHostEnvironment iWebHostEnvironment)
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
                                     .Where(A => A.Id == id)
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

        #region Users -------------------------------------------------------------OK
        /// <summary>
        /// اطلاعات کاربر
        /// </summary>
        [Route("user")]
        [HttpGet]
        public async Task<IActionResult> User()
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Users
                                         .Where(A => A.Id == User.Id)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             fName = A.Fname,
                                             lName = A.Lname,
                                             description = A.Description,
                                             lastLogin = A.LastLogin,
                                             mobile = A.Mobile,
                                             customer = A.Customer == null ? "" : A.Customer.Title,
                                             reserveTime = A.Customer == null ? "00:00:00" : A.Customer.ReserveTime.ToString(),
                                             showPrice = A.Customer == null ? false : A.Customer.ShowPrice,
                                             cancelTime = A.Customer == null ? "00:00:00" : A.Customer.CancelTime.ToString()
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
        /// تغییر رمز کاربر
        /// </summary>
        [Route("changeUserPassword")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserPassword([FromBody] UserChangePasswordRequest info)
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Users
                                        .Where(A => A.Id == User.Id)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "کاربر یافت نشد"));

                var PasswordVerified = BCrypt.Net.BCrypt.Verify(info.oldPassword, User.Password);
                if (PasswordVerified == false)
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(_Message: "رمز قبلی وارد شده اشتباه است"));

                data.Password = BCrypt.Net.BCrypt.HashPassword(info.newPassword);

                _NarijeDBContext.Users.Update(data);
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

        #region Report --------------------------------------------------------
        /// <summary>
        ///  لیست رزرو ها
        ///  ترتیب
        ///  1 بر اساس تاریخ صعودی
        ///  2 بر اساس تاریخ نزولی
        ///  3 بر اساس تعداد صعودی
        ///  4 بر اساس تعداد نزولی
        ///  </summary>
        [Route("reservesReport")]
        [HttpGet]
        public async Task<IActionResult> ReservesReport([FromQuery] int? Page, int? Limit, string order, string search, DateTime? fromDate, DateTime? toDate,
                                                            int? foodId, int? state)
        {
            if ((Page is null) || (Page == 0))
                Page = 1;
            if ((Limit is null) || (Limit == 0))
                Limit = 30;
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                bool ShowPrice = (User.Customer == null) ? false : User.Customer.ShowPrice;

                var Q = _NarijeDBContext.Reserves
                                         .Where(A => A.UserId == User.Id)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             state = A.State,
                                             foodId = A.FoodId,
                                             foodDescription = A.Food.Description,
                                             food = A.Food.Title,
                                             price = (ShowPrice == true) ? A.Price : 0, 
                                             foodGroupId = A.Food.GroupId,
                                             foodGroup = A.Food.Group.Title,
                                             isFood = A.Food.IsFood,
                                             qty = A.Num,
                                             createdAt = A.CreatedAt
                                         });

                if (search is not null)
                {
                    Q = Q.Where(A => A.food.Contains(search) || A.foodGroup.Contains(search));
                }

                if (fromDate is not null)
                {
                    Q = Q.Where(A => A.dateTime.Date >= fromDate.Value.Date);
                }
                if (toDate is not null)
                {
                    Q = Q.Where(A => A.dateTime.Date <= toDate.Value.Date);
                }
                if (foodId is not null)
                {
                    Q = Q.Where(A => A.foodId == foodId);
                }
                if (state is not null)
                {
                    Q = Q.Where(A => A.state == state);
                }

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


    }

}