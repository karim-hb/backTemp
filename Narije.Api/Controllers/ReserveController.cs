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
using Microsoft.Extensions.Caching.Memory;

namespace Narije.Api.Controllers
{
    /// <summary>
    /// Home
    /// </summary>
    [Route("user")]
    [Authorize(Roles = "user")]
    [ApiController]
    public class ReserveController : ControllerBase
    {
        private readonly NarijeDBContext _NarijeDBContext;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IConfiguration _IConfiguration;
        private readonly IHttpClientFactory _IHttpClientFactory;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        private static string BucketServiceURL = "https://tahlilmobile-gallery.storage.iran.liara.space/";
        private readonly IMemoryCache _memoryCache;
        /// <summary>
        /// متد سازنده
        /// </summary>
        public ReserveController(NarijeDBContext NarijeDBContext, IHttpContextAccessor iHttpContextAccessor, IConfiguration iConfiguration, IHttpClientFactory iHttpClientFactory, IWebHostEnvironment iWebHostEnvironment, IMemoryCache memoryCache)
        {
            _NarijeDBContext = NarijeDBContext;
            _IHttpContextAccessor = iHttpContextAccessor;
            _IConfiguration = iConfiguration;
            _IHttpClientFactory = iHttpClientFactory;
            _IWebHostEnvironment = iWebHostEnvironment;
            _memoryCache = memoryCache;
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
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Menus
                                        .Where(A => A.CustomerId == User.CustomerId)
                                        .Select(A => A.DateTime)
                                        .Distinct();

                if (fromDate != null)
                    Q = Q.Where(A => A.Date >= fromDate.Value.Date);
                if (toDate != null)
                    Q = Q.Where(A => A.Date <= toDate.Value.Date);

                var menus = await Q.Select(A => new MenuDayResponse()
                {
                    datetime = A
                }).ToListAsync();

                foreach (var item in menus)
                {
                    item.foods = await _NarijeDBContext.Menus
                                        .Where(A => A.CustomerId == User.CustomerId && A.DateTime.Date == item.datetime)
                                        .Select(A => new MenuFoodResponse()
                                        {
                                            maxReserve = A.MaxReserve,
                                            foodId = A.FoodId,
                                            food = A.Food.Title,
                                            foodDescription = A.Food.Description,
                                            foodGroupId = A.Food.GroupId,
                                            foodGroup = A.Food.Group.Title,
                                            image = A.Food.Gallery == null ? "" : $"{BucketServiceURL}{A.Food.GalleryId}",
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

        #region Reserve -------------------------------------------------------------
        /// <summary>
        /// مشاهده رزرو
        /// week 0 هفته جاری
        /// </summary>
        [Route("reserves")]
        [HttpGet]
        public async Task<IActionResult> Reserves([FromQuery] DateTime fromDate, DateTime toDate, string search, int? groupId, int mealId , int month , int year)
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var StartWeek = fromDate;
                var EndWeek = toDate;

                ReserveHelper helper = new();
                var reserves = await helper.GetUserReserve(_NarijeDBContext, User, StartWeek, EndWeek, search, groupId, BucketServiceURL, 0, mealId, month , year);///-1 

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: reserves));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));

            }
        }

        /// <summary>
        /// ویرایش رزرو
        /// </summary>
        [Route("reserves")]
        [HttpPut]
        public async Task<IActionResult> EditReserves([FromBody] ReservesRequest days)
        {
            try
            {
            

                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                if (_memoryCache != null && _memoryCache.TryGetValue(User.Id, out bool _))
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiErrorResponse(_Message: "در هر  ثانیه تنها یک درخواست مجاز به ارسال می باشید"));
                }
                _memoryCache.Set(User.Id, true, TimeSpan.FromSeconds(1));

                ReserveHelper helper = new();

                var result = await helper.Reserve(_NarijeDBContext, User, days, (int)EnumReserveState.normal, false);

                if(result == "")
                    return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: null));
                else
                    return StatusCode(StatusCodes.Status400BadRequest, new ApiErrorResponse(_Message: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }
        #endregion

    }

}