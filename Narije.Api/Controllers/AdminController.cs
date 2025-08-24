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
using Castle.Core.Resource;
using Hangfire.Server;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Narije.Core.DTOs.User;
using Org.BouncyCastle.Utilities;
using System.Globalization;

namespace Narije.Api.Controllers
{
    /// <summary>
    /// Home
    /// </summary>
    [Route("admin")]
    [Authorize(Roles = "supervisor")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly NarijeDBContext _NarijeDBContext;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IConfiguration _IConfiguration;
        private readonly IHttpClientFactory _IHttpClientFactory;
        private readonly IWebHostEnvironment _IWebHostEnvironment;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public AdminController(NarijeDBContext NarijeDBContext, IHttpContextAccessor iHttpContextAccessor, IConfiguration iConfiguration, IHttpClientFactory iHttpClientFactory, IWebHostEnvironment iWebHostEnvironment)
        {
            _NarijeDBContext = NarijeDBContext;
            _IHttpContextAccessor = iHttpContextAccessor;
            _IConfiguration = iConfiguration;
            _IHttpClientFactory = iHttpClientFactory;
            _IWebHostEnvironment = iWebHostEnvironment;
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

        #region Food -------------------------------------------------------------
        /// <summary>
        /// شمارش غذاها
        /// </summary>
        [Route("foodCount")]
        [HttpGet]
        public async Task<IActionResult> FoodCount()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Foods
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
        /// لیست غذاها
        /// </summary>
        [Route("foods")]
        [HttpGet]
        public async Task<IActionResult> Foods([FromQuery] int? Page, [FromQuery] int? Limit, [FromQuery] int? groupId, [FromQuery] string search, [FromQuery] int? customerId
                                                , [FromQuery] bool? isGuest, [FromQuery] bool? active)
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

                var Q = _NarijeDBContext.Foods
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
                                            groupId = A.GroupId,
                                            group = A.Group == null ? "" : A.Group.Title,
                                            image = A.GalleryId,
                                            isDaily = A.IsDaily,
                                            hasType = A.HasType,
                                            isGuest = A.IsGuest,
                                            vat = A.Vat,
                                            echoPrice = customerId == null ? A.EchoPrice : A.FoodPrices.Where(A => A.CustomerId == customerId).Select(A => A.EchoPrice).FirstOrDefault(),
                                            specialPrice = customerId == null ? A.SpecialPrice : A.FoodPrices.Where(A => A.CustomerId == customerId).Select(A => A.SpecialPrice).FirstOrDefault(),
                                            active = A.Active
                                        })
                                        .AsNoTracking();

                if (groupId != null)
                    Q = Q.Where(A => A.groupId == groupId);
                if (isGuest != null)
                    Q = Q.Where(A => A.isGuest == isGuest);
                if (active != null)
                    Q = Q.Where(A => A.active == active);

                if (search != null)
                    Q = Q.Where(A => A.title.Contains(search));

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
        /// ویرایش غذا
        /// </summary>
        [Route("food")]
        [HttpPut]
        public async Task<IActionResult> EditFood([FromForm] FoodRequest food)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var group = await _NarijeDBContext.FoodGroups.Where(A => A.Id == food.groupId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (group is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "گروه کالا غذا یافت نشد"));

                var data = await _NarijeDBContext.Foods.Where(A => A.Id == food.id)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "غذا یافت نشد"));

                data.Title = food.title;
                data.IsDaily = food.isDaily;
                data.Active = food.active;
                data.GroupId = food.groupId;
                data.HasType = food.hasType;
                data.IsGuest = food.isGuest;
                data.EchoPrice = food.echoPrice;
                data.SpecialPrice = food.specialPrice;
                data.Vat = food.vat;

                data.GalleryId = await EditFromGallery(data.GalleryId, food.fromGallery);

                if (food.files != null)
                {
                    data.GalleryId = await EditGallery(data.GalleryId, "FOOD", food.files.FirstOrDefault());
                }

                _NarijeDBContext.Foods.Update(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    title = data.Title,
                    groupId = data.GroupId,
                    group = data.Group == null ? "" : data.Group.Title,
                    image = data.GalleryId,
                    isDaily = data.IsDaily,
                    hasType = data.HasType,
                    isGuest = data.IsGuest,
                    active = data.Active,
                    echoPrice = data.EchoPrice,
                    specialPrice = data.SpecialPrice,
                    vat = data.Vat
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
        /// افزودن غذا
        /// </summary>
        [Route("food")]
        [HttpPost]
        public async Task<IActionResult> AddFood([FromForm] FoodRequest food)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var group = await _NarijeDBContext.FoodGroups.Where(A => A.Id == food.groupId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (group is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "گروه کالا غذا یافت نشد"));

                var data = new Food
                {
                    Title = food.title,
                    Active = food.active,
                    IsDaily = food.isDaily,
                    GroupId = food.groupId,
                    HasType = food.hasType,
                    IsGuest = food.isGuest,
                    SpecialPrice = food.specialPrice,
                    EchoPrice = food.echoPrice,
                    Vat = food.vat
                };

                data.GalleryId = await AddFromGallery(food.fromGallery);
                if (food.files != null)
                {
                    var k = await AddToGallery("FOOD", food.files.FirstOrDefault());
                    if (k > 0)
                        data.GalleryId = k;
                }

                await _NarijeDBContext.Foods.AddAsync(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    title = data.Title,
                    groupId = data.GroupId,
                    group = data.Group == null ? "" : data.Group.Title,
                    image = data.GalleryId,
                    isDaily = data.IsDaily,
                    hasType = data.HasType,
                    isGuest = data.IsGuest,
                    active = data.Active,
                    echoPrice = data.EchoPrice,
                    specialPrice = data.SpecialPrice,
                    vat = data.Vat
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
        /// حذف غذا
        /// </summary>
        [Route("food/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteFood([FromRoute] int id)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var exists = await _NarijeDBContext.Reserves
                                         .Where(A => A.FoodId == id)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync();

                if (exists is not null)
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "غذای رزرو شده قابل حذف نیست"));

                var food = await _NarijeDBContext.Foods.Where(A => A.Id == id)
                                         .FirstOrDefaultAsync();
                if (food is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "غذا یافت نشد"));

                var prices = await _NarijeDBContext.FoodPrices.Where(A => A.FoodId == id)
                                         .ToListAsync();
                _NarijeDBContext.FoodPrices.RemoveRange(prices);
                var menus = await _NarijeDBContext.Menus.Where(A => A.FoodId == id)
                                         .ToListAsync();
                _NarijeDBContext.Menus.RemoveRange(menus);

                _NarijeDBContext.Foods.Remove(food);
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

        #region Province -------------------------------------------------------------
        /// <summary>
        /// لیست استان ها
        /// </summary>
        [Route("provinces")]
        [HttpGet]
        public async Task<IActionResult> Provinces([FromQuery] int? Page, [FromQuery] int? Limit)
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

                var Q = _NarijeDBContext.Provinces
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
                                            cities = A.Cities.Select(B => new
                                            {
                                                id = B.Id,
                                                title = B.Title,
                                            }).ToList()
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

        #region City -------------------------------------------------------------
        /// <summary>
        /// لیست شهر ها
        /// </summary>
        [Route("cities")]
        [HttpGet]
        public async Task<IActionResult> Cities([FromQuery] int? Page, [FromQuery] int? Limit)
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

                var Q = _NarijeDBContext.Cities
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
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

        #region Groups -------------------------------------------------------------
        /// <summary>
        /// شمارش گروه ها
        /// </summary>
        [Route("groupsCount")]
        [HttpGet]
        public async Task<IActionResult> GroupCount()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.FoodGroups
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

        /// <summary>
        /// ویرایش گروه
        /// </summary>
        [Route("group")]
        [HttpPut]
        public async Task<IActionResult> EditGroup([FromForm] FoodGroupRequest group)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));


                var data = await _NarijeDBContext.FoodGroups.Where(A => A.Id == group.id)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "گروه کالا غذا یافت نشد"));

                data.Title = group.title;

                data.GalleryId = await EditFromGallery(data.GalleryId, group.fromGallery);

                if (group.files != null)
                {
                    data.GalleryId = await EditGallery(data.GalleryId, "FOODGROUP", group.files.FirstOrDefault());
                }

                _NarijeDBContext.FoodGroups.Update(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    title = data.Title,
                    image = data.GalleryId
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
        /// افزودن گروه
        /// </summary>
        [Route("group")]
        [HttpPost]
        public async Task<IActionResult> AddGroup([FromForm] FoodGroupRequest group)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));


                var data = new FoodGroup
                {
                    Title = group.title,
                };

                data.GalleryId = await AddFromGallery(group.fromGallery);
                if (group.files != null)
                {
                    var k = await AddToGallery("FOODGROUP", group.files.FirstOrDefault());
                    if (k > 0)
                        data.GalleryId = k;
                }

                await _NarijeDBContext.FoodGroups.AddAsync(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    title = data.Title,
                    image = data.GalleryId
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
        /// حذف گروه
        /// </summary>
        [Route("group/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteGroup([FromRoute] int id)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.FoodGroups.Where(A => A.Id == id)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "گروه کالا غذا یافت نشد"));

                _NarijeDBContext.FoodGroups.Remove(data);
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

        #region Setting -------------------------------------------------------------
        /// <summary>
        /// تنظیمات
        /// </summary>
        [Route("setting")]
        [HttpGet]
        public async Task<IActionResult> Setting()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Settings
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            companyName = A.CompanyName,
                                            contactMobile = A.ContactMobile,
                                            companyGalleryId = A.CompanyGalleryId,
                                            companyDarkGalleryId = A.CompanyDarkGalleryId,
                                            tel = A.Tel,
                                            address = A.Address,
                                            economicCode = A.EconomicCode,
                                            nationalId = A.NationalId,
                                            regNumber = A.RegNumber,
                                            postalCode = A.PostalCode,
                                            cityId = A.CityId,
                                            city = A.City == null ? "" : A.City.Title,
                                            provinceId = A.ProvinceId,
                                            province = A.Province == null ? "" : A.Province.Title
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
        /// ویرایش تنظیمات
        /// </summary>
        [Route("setting")]
        [HttpPut]
        public async Task<IActionResult> EditSetting([FromForm] SettingRequest setting)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Settings
                                        .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "اطلاعات شرکت یافت نشد"));

                data.CompanyName = setting.companyName;
                data.Tel = setting.tel;
                data.Address = setting.address;
                data.EconomicCode = setting.economicCode;
                data.PostalCode = setting.postalCode;
                data.CityId = setting.cityId;
                data.ProvinceId = setting.provinceId;
                data.ContactMobile = setting.contactMobile;
                data.RegNumber = setting.regNumber;
                data.NationalId = setting.nationalId;

                data.CompanyGalleryId = await EditFromGallery(data.CompanyGalleryId, setting.fromGallery);

                if (setting.fromGalleryDarkLogo != null)
                {
                    data.CompanyDarkGalleryId = await EditFromGallery(data.CompanyDarkGalleryId, setting.fromGalleryDarkLogo);
                }

                if (setting.files != null)
                {
                    data.CompanyGalleryId = await EditGallery(data.CompanyGalleryId, "SETTING", setting.files.FirstOrDefault());
                }

                if (setting.darkLogoFiles != null)
                {
                    data.CompanyDarkGalleryId = await EditGallery(data.CompanyDarkGalleryId, "SETTING", setting.darkLogoFiles.FirstOrDefault());
                }

                _NarijeDBContext.Settings.Update(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    companyName = data.CompanyName,
                    contactMobile = data.ContactMobile,
                    companyGalleryId = data.CompanyGalleryId,
                    companyDarkGalleryId = data.CompanyDarkGalleryId,
                    tel = data.Tel,
                    address = data.Address,
                    economicCode = data.EconomicCode,
                    nationalId = data.NationalId,
                    regNumber = data.RegNumber,
                    postalCode = data.PostalCode,
                    cityId = data.CityId,
                    city = data.City == null ? "" : data.City.Title,
                    provinceId = data.ProvinceId,
                    province = data.Province == null ? "" : data.Province.Title
                };

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: result));
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
        /// شمارش مشتری ها
        /// </summary>
        [Route("customersCount")]
        [HttpGet]
        public async Task<IActionResult> CustomersCount()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Customers
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
        /// لیست مشتری ها
        /// </summary>
        [Route("customers")]
        [HttpGet]
        public async Task<IActionResult> Customers([FromQuery] int? Page, [FromQuery] int? Limit, [FromQuery] string search)
        {
            try
            {
                if ((Page is null) || (Page == 0))
                    Page = 1;
                if (Limit is null)
                    Limit = 30;

                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.VCustomers
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
                                            minReserve = A.MinReserve,
                                            economicCode = A.EconomicCode,
                                            nationalId = A.NationalId,
                                            regNumber = A.RegNumber,
                                            postalCode = A.PostalCode,
                                            mobile = A.Mobile,
                                            cityId = A.CityId,
                                            city = A.City,
                                            provinceId = A.ProvinceId,
                                            province = A.Province,
                                            parentId = A.ParentId,
                                        })
                                        .AsNoTracking();

                if (search != null)
                {
                    Q = Q.Where(A => A.title.Contains(search) || A.tel.Contains(search));
                }

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
        /// ویرایش مشتری
        /// </summary>
        [Route("customer")]
        [HttpPut]
        public async Task<IActionResult> EditCustomer([FromBody] CustomerRequest customer)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Customers.Where(A => A.Id == customer.id)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "اطلاعات شرکت یافت نشد"));

                data.Title = customer.title;
                data.Tel = customer.tel;
                data.ReserveTo = customer.reserveTo;
                data.ReserveAfter = 0;  //customer.reserveAfter;
                data.Active = customer.active;
                data.Address = customer.address;
                data.ShowPrice = customer.showPrice;
                data.CancelPercent = 0;// customer.cancelPercent;
                data.FoodType = customer.foodType;
                data.CancelPercentPeriod = 0;// customer.cancelPercentPeriod;
                if (customer.reserveTime != null)
                    data.ReserveTime = TimeSpan.Parse(customer.reserveTime);
                //if (customer.cancelTime != null)
                data.CancelTime = TimeSpan.Parse("10:00:00");
                data.ContractStartDate = customer.contractStartDate;
                if (customer.guestTime != null)
                    data.GuestTime = TimeSpan.Parse(customer.guestTime);
                data.MinReserve = customer.minReserve;
                data.EconomicCode = customer.economicCode;
                data.PostalCode = customer.postalCode;
                data.CityId = customer.cityId;
                data.ProvinceId = customer.provinceId;
                data.Mobile = customer.mobile;
                data.RegNumber = customer.regNumber;
                data.NationalId = customer.nationalId;
                data.ParentId = customer.parentId;


                _NarijeDBContext.Customers.Update(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    title = data.Title,
                    tel = data.Tel,
                    contractStartDate = data.ContractStartDate,
                    address = data.Address,
                    //cancelPercent = data.CancelPercent,
                    //cancelPercentPeriod = data.CancelPercentPeriod,
                    //cancelTime = data.CancelTime,
                    guestTime = data.GuestTime,
                    //reserveAfter = data.ReserveAfter,
                    reserveTo = data.ReserveTo,
                    active = data.Active,
                    showPrice = data.ShowPrice,
                    foodType = data.FoodType,
                    reserveTime = data.ReserveTime,
                    economicCode = data.EconomicCode,
                    nationalId = data.NationalId,
                    regNumber = data.RegNumber,
                    postalCode = data.PostalCode,
                    mobile = data.Mobile,
                    cityId = data.CityId,
                    city = data.City == null ? "" : data.City.Title,
                    provinceId = data.ProvinceId,
                    province = data.Province == null ? "" : data.Province.Title,
                    parentId = data.ParentId
                };

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /*
        /// <summary>
        /// افزودن مشتری
        /// </summary>
        [Route("customer")]
        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerRequest customer)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = new Customer();
                data.Title = customer.title;
                data.Tel = customer.tel;
                data.ReserveTo = 0;// customer.reserveTo;
                data.ReserveAfter = 0;// customer.reserveAfter;
                data.Active = customer.active;
                data.Address = customer.address;
                data.CancelPercent = 0;// customer.cancelPercent;
                data.ShowPrice = customer.showPrice;
                data.CancelPercentPeriod = 0;// customer.cancelPercentPeriod;
                data.FoodType = customer.foodType;
                if (customer.reserveTime != null)
                    data.ReserveTime = TimeSpan.Parse(customer.reserveTime);
                //if (customer.cancelTime != null)
                data.CancelTime = TimeSpan.Parse("10:00:00");
                data.ContractStartDate = customer.contractStartDate;
                data.MinReserve = customer.minReserve;
                if(customer.guestTime != null)
                    data.GuestTime = TimeSpan.Parse(customer.guestTime);
                data.EconomicCode = customer.economicCode;
                data.PostalCode = customer.postalCode;
                data.CityId = customer.cityId;
                data.ProvinceId = customer.provinceId;
                data.Mobile = customer.mobile;
                data.RegNumber = customer.regNumber;
                data.NationalId = customer.nationalId;
                data.ParentId = customer.parentId;

                await _NarijeDBContext.Customers.AddAsync(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    title = data.Title,
                    tel = data.Tel,
                    contractStartDate = data.ContractStartDate,
                    address = data.Address,
                    //cancelPercent = data.CancelPercent,
                    //cancelPercentPeriod = data.CancelPercentPeriod,
                    //cancelTime = data.CancelTime,
                    guestTime = data.GuestTime,
                    //reserveAfter = data.ReserveAfter,
                    reserveTo = data.ReserveTo,
                    active = data.Active,
                    foodType = data.FoodType,
                    reserveTime = data.ReserveTime,
                    showPrice = data.ShowPrice,
                    economicCode = data.EconomicCode,
                    nationalId = data.NationalId,
                    regNumber = data.RegNumber,
                    postalCode = data.PostalCode,
                    mobile = data.Mobile,
                    cityId = data.CityId,
                    city = data.City == null ? "" : data.City.Title,
                    provinceId = data.ProvinceId,
                    province = data.Province == null ? "" : data.Province.Title,
                    parentId = data.ParentId
                };

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        */

        /// <summary>
        /// حذف مشتری
        /// </summary>
        [Route("customer/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteCustomer([FromRoute] int id)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Customers.Where(A => A.Id == id)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync();
                if (data is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "اطلاعات شرکت یافت نشد"));

                _NarijeDBContext.Customers.Remove(data);
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
        public async Task<IActionResult> Users([FromQuery] int? Page, int? Limit, string search, int? role, string order, int? customerId)
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
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             fName = A.Fname,
                                             description = A.Description,
                                             lName = A.Lname,
                                             lastLogin = A.LastLogin,
                                             mobile = A.Mobile,
                                             role = A.Role,
                                             active = A.Active,
                                             customerId = A.CustomerId,
                                             customer = A.Customer == null ? "" : A.Customer.Title
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

                if (customerId != null)
                    Q = Q.Where(A => A.customerId == customerId);

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
                                         .Where(A => A.Id == id)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             fName = A.Fname,
                                             description = A.Description,
                                             lName = A.Lname,
                                             lastLogin = A.LastLogin,
                                             mobile = A.Mobile,
                                             role = A.Role,
                                             active = A.Active,
                                             customerId = A.CustomerId,
                                             customer = A.Customer == null ? "" : A.Customer.Title
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

                var data = await _NarijeDBContext.Users.Where(A => A.Id == user.id)
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
                data.Description = user.description;
                data.Lname = user.lName;
                data.Mobile = user.mobile;
                data.CustomerId = user.customerId;
                if (user.password != null)
                    data.Password = BCrypt.Net.BCrypt.HashPassword(user.password);
                if (user.active != null)
                    data.Active = user.active.Value;

                _NarijeDBContext.Users.Update(data);
                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = data.Id,
                    fName = data.Fname,
                    description = data.Description,
                    lName = data.Lname,
                    mobile = data.Mobile,
                    role = data.Role,
                    active = data.Active,
                    customerId = data.CustomerId,
                    customer = data.Customer == null ? "" : data.Customer.Title
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
                data.Description = user.description;
                data.Lname = user.lName;
                data.Password = BCrypt.Net.BCrypt.HashPassword(user.password);
                data.Mobile = user.mobile;
                data.CustomerId = user.customerId;
                if ((Admin.Role == (int)EnumRole.supervisor) && (data.Id != Admin.Id))
                {
                    if ((user.role != null) && (user.role <= 3))
                        data.Role = user.role.Value;
                }
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
                    active = data.Active,
                    customerId = data.CustomerId,
                    customer = data.Customer == null ? "" : data.Customer.Title
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

                var data = await _NarijeDBContext.Users.Where(A => A.Id == id)
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
        [Authorize(Roles = "supervisor")]
        [Route("changeUserPassword")]
        [HttpPut]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordRequest info)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var User = await _NarijeDBContext.Users.Where(A => A.Id == info.id)
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

        #region FoodPrice -------------------------------------------------------------
        /// <summary>
        /// لیست قیمت یک مشتری
        /// </summary>
        [Route("foodPrices")]
        [HttpGet]
        public async Task<IActionResult> FoodPrices([FromQuery] int? Page, [FromQuery] int? Limit, [FromQuery] int customerId, [FromQuery] string search, [FromQuery] int? foodGroupId)
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

                var Q = _NarijeDBContext.Foods
                                        .Where(A => A.Active)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
                                            groupId = A.GroupId,
                                            group = A.Group.Title,
                                            hasType = A.HasType,
                                            echoPrice = A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id) == null ? 0 : A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id).Select(B => B.EchoPrice).FirstOrDefault(),
                                            specialPrice = A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id) == null ? 0 : A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id).Select(B => B.SpecialPrice).FirstOrDefault(),
                                        })
                                        .AsNoTracking();

                if (search != null)
                    Q = Q.Where(A => A.title.Contains(search) || A.group.Contains(search));
                if (foodGroupId != null)
                    Q = Q.Where(A => A.groupId == foodGroupId);

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
        /// ویرایش قیمت
        /// </summary>
        [Route("foodPrice")]
        [HttpPut]
        public async Task<IActionResult> EditPrice([FromBody] FoodPriceRequest price)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var customer = await _NarijeDBContext.Customers.Where(A => A.Id == price.customerId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (customer is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "اطلاعات شرکت یافت نشد"));

                var food = await _NarijeDBContext.Foods.Where(A => A.Id == price.foodId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (food is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "غذا یافت نشد"));

                var foodprice = await _NarijeDBContext.FoodPrices.Where(A => A.FoodId == price.foodId && A.CustomerId == price.customerId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

                if (foodprice is null)
                {
                    foodprice = new FoodPrice()
                    {
                        CustomerId = price.customerId,
                        FoodId = price.foodId,
                        EchoPrice = price.echoPrice,
                        SpecialPrice = price.specialPrice
                    };
                    await _NarijeDBContext.FoodPrices.AddAsync(foodprice);
                }
                else
                {
                    foodprice.EchoPrice = price.echoPrice;
                    foodprice.SpecialPrice = price.specialPrice;
                    _NarijeDBContext.FoodPrices.Update(foodprice);
                }

                await _NarijeDBContext.SaveChangesAsync();

                var result = new
                {
                    id = foodprice.Id,
                    echoPrice = foodprice.EchoPrice,
                    specialPrice = foodprice.SpecialPrice
                };

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: result));
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
        /// شمارش منوها
        /// </summary>
        [Route("menusCount")]
        [HttpGet]
        public async Task<IActionResult> MenusCount()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Customers
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
        /// اطلاعات منوی غذایی
        /// </summary>
        [Route("menu")]
        [HttpGet]
        public async Task<IActionResult> Menu([FromQuery] int? Page, [FromQuery] int? Limit, [FromQuery] int customerId, [FromQuery] DateTime date,
                                                    [FromQuery] string search, int? foodGroupId, int? mealId)
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

                var daymenu = await _NarijeDBContext.Menus
                                    .Where(A => A.CustomerId == customerId && A.DateTime.Date == date)
                                    .AsNoTracking()
                                    .ToListAsync();
                if (mealId.HasValue)
                {
                    daymenu = daymenu.Where(A => A.MealType == mealId.Value).ToList();
                }
                var customer = await _NarijeDBContext.Customers
                                    .Where(A => A.Id == customerId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

                int ftype = (int)EnumFoodType.special;
                if (customer != null)
                    ftype = customer.FoodType;

                var ids = daymenu.Select(A => A.FoodId).ToList();
                var Q = _NarijeDBContext.vCustomerFoodPrices
                                    .Where(A => A.Id == customer.Id)// && ids.Contains(A.FoodId))
                                    .Select(A => new MenuFoodResponse()
                                    {
                                        id = null,
                                        maxReserve = 0,
                                        foodId = A.FoodId,
                                        food = A.Title,
                                        foodDescription = A.Description,
                                        image = A.GalleryId.ToString(),
                                        foodGroupId = A.GroupId,
                                        foodGroup = A.GroupTitle,
                                        echoPrice = A.EchoPrice,
                                        specialPrice = A.SpecialPrice,
                                        foodType = 0,
                                        mealId = 0
                                    });
                if (foodGroupId != null)
                    Q = Q.Where(A => A.foodGroupId == foodGroupId);
                if (search != null)
                    Q = Q.Where(A => A.food.Contains(search));
                //Q = Q.Where(A => (A.echoPrice > 0 || A.specialPrice > 0));

                var MenuFoods = await Q.ToListAsync();

                foreach (var mf in MenuFoods)
                {
                    var sel = daymenu.Where(A => A.FoodId == mf.foodId).FirstOrDefault();
                    mf.foodType = ftype;

                    if (sel is null)
                    {
                        continue;
                    }
                    mf.maxReserve = sel.MaxReserve;
                    mf.id = sel.Id;
                    mf.mealId = sel.MealType;
                }

                var N = MenuFoods.AsQueryable();
                N = N.OrderByDescending(A => A.id);

                var data = N.GetPaged2(Page: Page.Value, Limit: Limit.Value);

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data.Data, _Meta: data.Meta));


                #region old
                /*
                var daymenu = await _NarijeDBContext.Menus
                                    .Where(A => A.CustomerId == customerId && A.DateTime.Date == date)
                                    .AsNoTracking()
                                    .ToListAsync();

                var customer = await _NarijeDBContext.Customers
                                    .Where(A => A.Id == customerId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

                int ftype = (int)EnumFoodType.special;
                if (customer != null)
                    ftype = customer.FoodType;

                var Q = _NarijeDBContext.Foods
                                    .Where(A => A.Active)
                                    .Select(A => new MenuFoodResponse()
                                    {
                                        id = null,
                                        maxReserve = 0,
                                        foodId = A.Id,
                                        food = A.Title,
                                        image = A.GalleryId.ToString(),
                                        foodGroupId = A.GroupId,
                                        foodGroup = A.Group.Title,
                                        echoPrice = A.FoodPrices == null ? null : A.FoodPrices.Where(A => A.CustomerId == customerId).Select(A => A.EchoPrice).FirstOrDefault(),
                                        specialPrice = A.FoodPrices == null ? null : A.FoodPrices.Where(A => A.CustomerId == customerId).Select(A => A.SpecialPrice).FirstOrDefault(),
                                        foodType = 0
                                    });
                if (foodGroupId != null)
                    Q = Q.Where(A => A.foodGroupId == foodGroupId);
                if (search != null)
                    Q = Q.Where(A => A.food.Contains(search));
                //Q = Q.Where(A => (A.echoPrice > 0 || A.specialPrice > 0));

                var MenuFoods = await Q.ToListAsync();

                var ids = MenuFoods.Select(A => A.foodId).ToList();
                var Foods = _NarijeDBContext.Foods.Where(A => ids.Contains(A.Id)).ToList();

                var ParentPrice = await _NarijeDBContext.FoodPrices
                                            .Where(A => A.CustomerId == customer.ParentId)
                                            .AsNoTracking()
                                            .ToListAsync();


                foreach (var mf in MenuFoods)
                {
                    var sel = daymenu.Where(A => A.FoodId == mf.foodId).FirstOrDefault();
                    var cp = ParentPrice.Where(A => A.FoodId == mf.foodId).FirstOrDefault();
                    mf.foodType = ftype;// sel.FoodType;

                    if (mf.echoPrice == 0)
                    {
                        if ((cp != null) && (cp.EchoPrice != 0))
                            mf.echoPrice = cp.EchoPrice;
                        else
                            mf.echoPrice = Foods.Where(A => A.Id == mf.foodId).Select(A => A.EchoPrice).FirstOrDefault();

                    }
                    if (mf.specialPrice == 0)
                    {
                        if ((cp != null) && (cp.SpecialPrice != 0))
                            mf.specialPrice = cp.SpecialPrice;
                        else
                            mf.specialPrice = Foods.Where(A => A.Id == mf.foodId).Select(A => A.SpecialPrice).FirstOrDefault();
                    }

                    if (sel is null)
                    {
                        continue;
                    }
                    mf.foodType = ftype;// sel.FoodType;
                    mf.maxReserve = sel.MaxReserve;
                    mf.id = sel.Id;
                }

                var N = MenuFoods.AsQueryable();
                N = N.OrderByDescending(A => A.id);

                var data = N.GetPaged2(Page: Page.Value, Limit: Limit.Value);

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data.Data, _Meta: data.Meta));

                */
                #endregion
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ویرایش منوی غذایی
        /// </summary>
        [Route("menu")]
        [HttpPut]
        public async Task<IActionResult> EditMenu([FromBody] MenuRequest menu)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var customer = await _NarijeDBContext.Customers.Where(A => A.Id == menu.customerId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (customer is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "اطلاعات شرکت یافت نشد"));




                List<int> mids = new List<int>();
                foreach (var item in menu.days)
                {
                    mids.AddRange(item.foods.Select(A => A.foodId).ToList());

                }
                var checkfoods = await _NarijeDBContext.vCustomerFoodPrices
                                    .Where(A => A.Id == customer.Id && mids.Contains(A.FoodId))
                                    .Select(A => new MenuFoodResponse()
                                    {
                                        foodId = A.FoodId,
                                        echoPrice = A.EchoPrice,
                                        specialPrice = A.SpecialPrice,
                                    }).ToListAsync();

                foreach (var item in menu.days)
                {
                    foreach (var f in item.foods)
                    {
                        var ff = checkfoods.Where(A => A.foodId == f.foodId).FirstOrDefault();
                        if (f.foodType == 0)
                        {
                            if ((ff == null) || (ff.echoPrice == 0))
                                return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "غذا با قیمت صفر در منو امکانپذیر نیست"));
                        }
                        else
                        {
                            if ((ff == null) || (ff.specialPrice == 0))
                                return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "غذا با قیمت صفر در منو امکانپذیر نیست"));
                        }
                    }
                }


                var foods = await _NarijeDBContext.Foods.Select(A => A.Id).ToListAsync();

                var menus = await _NarijeDBContext.Menus.Where(A => A.CustomerId == menu.customerId)
                                        .ToListAsync();

                foreach (var day in menu.days)
                {
                    var CurIds = day.foods.Select(A => A.foodId).ToList();
                    var mustdelete = menus.Where(A => A.DateTime.Date == day.datetime.Date && menu.mealId == A.MealType && !CurIds.Contains(A.FoodId)).ToList();
                    if (mustdelete.Count > 0)
                        _NarijeDBContext.Menus.RemoveRange(mustdelete);

                    foreach (var food in day.foods)
                    {
                        var id = foods.Where(A => A == food.foodId).FirstOrDefault();
                        if (id == 0)
                            return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "غذا یافت نشد"));


                        var item = menus.Where(A => A.DateTime.Date == day.datetime.Date && A.FoodId == food.foodId && menu.mealId == A.MealType).FirstOrDefault();
                        if (item != null)
                        {
                            if ((item.MaxReserve != food.maxReserve) || (item.FoodType != food.foodType))
                            {
                                item.MaxReserve = food.maxReserve;
                                item.FoodType = customer.FoodType;
                                item.MealType = menu.mealId;
                                _NarijeDBContext.Menus.Update(item);
                            }
                        }
                        else
                        {
                            item = new Menu();
                            item.DateTime = day.datetime.Date;
                            item.MaxReserve = food.maxReserve;
                            item.CustomerId = customer.Id;
                            item.FoodId = food.foodId;
                            item.FoodType = customer.FoodType;
                            item.MealType = menu.mealId;
                            await _NarijeDBContext.Menus.AddAsync(item);
                        }
                    }
                }


                await _NarijeDBContext.SaveChangesAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: null));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        #region Gallery ---------------------------------------------------------------------------
        /// <summary>
        /// شمارش تصاویر
        /// </summary> 
        [Route("galleryCount")]
        [HttpGet]
        public async Task<IActionResult> GalleryCount()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var data = await _NarijeDBContext.Galleries
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
        /// لیست تصاویر
        /// </summary>
        [Route("galley")]
        [HttpGet]
        public async Task<IActionResult> Gallery([FromQuery] int? Page, [FromQuery] int? Limit, [FromQuery] string search, [FromQuery] string filter)
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

                var Q = _NarijeDBContext.Galleries
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            fileName = A.OriginalFileName,
                                            source = A.Source,
                                            alt = A.Alt,
                                            extension = A.SystemFileName
                                        })
                                        .OrderByDescending(A => A.id)
                                        .AsNoTracking();

                if (filter != null)
                {
                    Q = Q.Where(A => A.extension.Contains(filter));
                }

                if (search != null)
                {
                    Q = Q.Where(A => A.alt.Contains(search) || A.fileName.Contains(search));
                }

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
        /// افزودن تصاویر
        /// </summary>
        [Route("addGalley")]
        [HttpPost]
        public async Task<IActionResult> AddGallery([FromForm] List<IFormFile> files)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));


                List<int> ids = new();
                foreach (var file in files)
                {
                    int id = await AddToGallery("Gallery", file);
                    if (id > 0)
                        ids.Add(id);
                }

                var data = await _NarijeDBContext.Galleries
                                        .Where(A => ids.Contains(A.Id))
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            fileName = A.OriginalFileName,
                                            source = A.Source,
                                            alt = A.Alt,
                                            extension = A.SystemFileName
                                        })
                                        .OrderByDescending(A => A.id)
                                        .AsNoTracking().ToListAsync();


                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError, Ex.Message);
            }
        }

        /// <summary>
        /// ویرایش تصویر
        /// </summary>
        [Route("Galley")]
        [HttpPut]
        public async Task<IActionResult> EditGallery([FromForm] GalleryRequest data)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == data.Id).AsNoTracking().FirstOrDefaultAsync();

                if (gallery == null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "تصویر یافت نشد"));

                if (data.OriginalFileName != null)
                    gallery.OriginalFileName = data.OriginalFileName;
                if (data.Source != null)
                    gallery.Source = data.Source;
                if (data.Alt != null)
                    gallery.Alt = data.Alt;

                _NarijeDBContext.Galleries.Update(gallery);
                await _NarijeDBContext.SaveChangesAsync();

                var result = await _NarijeDBContext.Galleries
                                        .Where(A => A.Id == gallery.Id)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            fileName = A.OriginalFileName,
                                            source = A.Source,
                                            alt = A.Alt,
                                            extension = A.SystemFileName
                                        })
                                        .AsNoTracking()
                                        .ToListAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// حذف تصویر
        /// </summary>
        [Route("gallery/{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteGallery([FromRoute] int id)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == id).AsNoTracking().FirstOrDefaultAsync();

                if (gallery == null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "تصویر یافت نشد"));

                var contentRoot = _IConfiguration.GetValue<string>(WebHostDefaults.ContentRootKey);
                var filepath = /*contentRoot +*/ "/data/" + string.Format("{0}{1}", gallery.Id, gallery.SystemFileName);

                try
                {
                    System.IO.File.Delete(filepath);
                }
                catch
                {
                }


                _NarijeDBContext.Galleries.Remove(gallery);
                await _NarijeDBContext.SaveChangesAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCESS"));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<int> AddToGallery(string Source, IFormFile file)
        {
            var contentRoot = _IConfiguration.GetValue<string>(WebHostDefaults.ContentRootKey);
            var path = /*contentRoot +*/ "/data/";

            if (file is null)
                return 0;

            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                var gallery = new Gallery()
                {
                    Source = Source,
                    OriginalFileName = file.FileName,
                    SystemFileName = extension
                };
                await _NarijeDBContext.Galleries.AddAsync(gallery);
                await _NarijeDBContext.SaveChangesAsync();

                string SysFileName = string.Format("{0}{1}", gallery.Id, gallery.SystemFileName);

                var filePath = path + SysFileName;

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                return gallery.Id;

            }

            return 0;

        }

        private async Task<int?> EditGallery(int? Id, string Source, IFormFile file)
        {
            var contentRoot = _IConfiguration.GetValue<string>(WebHostDefaults.ContentRootKey);
            var path = "/data/";

            Gallery gallery = null;

            if (Id != null)
                gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == Id).AsNoTracking().FirstOrDefaultAsync();

            if (gallery is null)
            {
                gallery = new Gallery()
                {
                    Source = Source,
                    OriginalFileName = "",
                    SystemFileName = ""
                };
            }

            if (file is null)
                return Id;

            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);

                gallery.OriginalFileName = file.FileName;
                gallery.SystemFileName = extension;

                if (gallery.Id == 0)
                    await _NarijeDBContext.Galleries.AddAsync(gallery);
                else
                    _NarijeDBContext.Galleries.Update(gallery);
                await _NarijeDBContext.SaveChangesAsync();
                Id = gallery.Id;

                string SysFileName = string.Format("{0}{1}", gallery.Id, gallery.SystemFileName);
                var filePath = path + SysFileName;

                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }
                return Id;
            }

            return Id;

        }

        private async Task<int?> EditFromGallery(int? Id, string ids)
        {

            if (ids == null)
                return Id;

            if (ids == "")
                return Id;

            var id = ids.Split(",");
            if (id.Count() == 0)
                return Id;

            int n = Int32.Parse(id.FirstOrDefault());

            var gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == n).AsNoTracking().FirstOrDefaultAsync();

            if (gallery is null)
                return Id;

            return gallery.Id;

        }

        private async Task<int?> AddFromGallery(string ids)
        {

            if (ids == null)
                return null;

            if (ids == "")
                return null;

            var id = ids.Split(",");
            if (id.Count() == 0)
                return null;

            int n = Int32.Parse(id.FirstOrDefault());

            var gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == n).AsNoTracking().FirstOrDefaultAsync();

            if (gallery is null)
                return null;

            return gallery.Id;

        }
        #endregion

        #region Reports
        /// <summary>
        ///  گزارش کلی
        ///  ترتیب
        ///  1 بر اساس تاریخ صعودی
        ///  2 بر اساس تاریخ نزولی
        ///  3 بر اساس تعداد صعودی
        ///  4 بر اساس تعداد نزولی
        ///  5 بر اساس نام و نام خانوادگی صعودی
        ///  6 بر اساس نام و نام خانوادگی نزولی
        ///  </summary>
        [Route("reserves")]
        [HttpGet]
        public async Task<IActionResult> Reserves([FromQuery] int? Page, int? Limit, string order, string search, DateTime? fromDate, DateTime? toDate,
                                                            int? foodId, int? foodGroupId, int? customerId, int? userId, int? state)
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
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             state = A.State,
                                             userId = A.UserId,
                                             userName = A.User.Fname + " " + A.User.Lname,
                                             customer = A.Customer.Title,
                                             customerId = A.CustomerId,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             foodDescription = A.Food.Description,
                                             foodGroupId = A.Food.GroupId,
                                             foodGroup = A.Food.Group.Title,
                                             isFood = A.Food.IsFood,
                                             foodType = A.FoodType,
                                             price = A.Price,
                                             qty = A.Num
                                         });

                if (search is not null)
                {
                    Q = Q.Where(A => A.userName.Contains(search) || A.food.Contains(search) || A.customer.Contains(search) || A.foodGroup.Contains(search));
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
                if (customerId is not null)
                {
                    Q = Q.Where(A => A.customerId == customerId);
                }
                if (foodGroupId is not null)
                {
                    Q = Q.Where(A => A.foodGroupId == foodGroupId);
                }
                if (userId is not null)
                {
                    Q = Q.Where(A => A.userId == userId);
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

        /// <summary>
        ///  لیست رزرو به تفکیک روز
        ///  </summary>
        [Route("reservesByDate")]
        [HttpGet]
        public async Task<IActionResult> ReservesByDate([FromQuery] int? Page, int? Limit, string order, string search, DateTime? fromDate, DateTime? toDate,
                                                            int? foodId, int? customerId, int? userId, int? state)
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

                var D = _NarijeDBContext.Reserves
                                         .Select(A => A.DateTime.Date).Distinct();

                if (fromDate is not null)
                    D = D.Where(A => A.Date >= fromDate.Value.Date);
                if (toDate is not null)
                    D = D.Where(A => A.Date <= toDate.Value.Date);

                if (order is not null)
                {
                    switch (order)
                    {
                        case "1":  //بر اساس تاریخ صعودی
                            D = D.OrderBy(A => A.Date);
                            break;
                        case "2":  //بر اساس تاریخ نزولی
                            D = D.OrderByDescending(A => A.Date);
                            break;
                    }
                }
                else
                    D = D.OrderByDescending(A => A.Date);

                var dates = await D.ToListAsync();

                var Q = _NarijeDBContext.Reserves
                                         .Where(A => dates.Contains(A.DateTime.Date))
                                         .Select(A => new ReportResponse()
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             state = A.State,
                                             userId = A.UserId,
                                             userName = A.User.Fname + " " + A.User.Lname,
                                             foodDescription = A.Food.Description,
                                             customer = A.Customer.Title,
                                             customerId = A.CustomerId,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             foodGroupId = A.Food.GroupId,
                                             foodGroup = A.Food.Group.Title,
                                             isFood = A.Food.IsFood,
                                             qty = A.Num
                                         });

                if (search is not null)
                    Q = Q.Where(A => A.userName.Contains(search) || A.food.Contains(search) || A.customer.Contains(search) || A.foodGroup.Contains(search));
                if (foodId is not null)
                    Q = Q.Where(A => A.foodId == foodId);
                if (customerId is not null)
                    Q = Q.Where(A => A.customerId == customerId);
                if (userId is not null)
                    Q = Q.Where(A => A.userId == userId);
                if (state is not null)
                    Q = Q.Where(A => A.state == state);

                var reserves = await Q.ToListAsync();

                var data = dates.Select(B => new ReportByDateResponse()
                {
                    datetime = B.Date,
                    items = reserves.Where(A => A.dateTime.Date == B.Date.Date).ToList()
                });

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        ///  لیست رزرو به تفکیک غذا
        ///  </summary>
        [Route("reservesByFood")]
        [HttpGet]
        public async Task<IActionResult> ReservesByFood([FromQuery] int? Page, int? Limit, string search, DateTime? fromDate, DateTime? toDate,
                                                            int? customerId, int? userId, int? state)
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

                var ids = await _NarijeDBContext.Foods.Select(A => new { id = A.Id, title = A.Title }).ToListAsync();

                var Q = _NarijeDBContext.Reserves
                                         .Where(A => ids.Select(A => A.id).Contains(A.FoodId))
                                         .Select(A => new ReportResponse()
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             state = A.State,
                                             userId = A.UserId,
                                             userName = A.User.Fname + " " + A.User.Lname,
                                             customer = A.Customer.Title,
                                             customerId = A.CustomerId,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             foodDescription = A.Food.Description,
                                             foodGroupId = A.Food.GroupId,
                                             foodGroup = A.Food.Group.Title,
                                             isFood = A.Food.IsFood,
                                             qty = A.Num
                                         });

                if (search is not null)
                    Q = Q.Where(A => A.userName.Contains(search) || A.food.Contains(search) || A.customer.Contains(search) || A.foodGroup.Contains(search));
                if (customerId is not null)
                    Q = Q.Where(A => A.customerId == customerId);
                if (userId is not null)
                    Q = Q.Where(A => A.userId == userId);
                if (state is not null)
                    Q = Q.Where(A => A.state == state);
                if (fromDate is not null)
                    Q = Q.Where(A => A.dateTime.Date >= fromDate.Value.Date);
                if (toDate is not null)
                    Q = Q.Where(A => A.dateTime.Date <= toDate.Value.Date);

                var reserves = await Q.ToListAsync();

                var hasreserves = reserves.Select(A => A.foodId).Distinct().ToList();

                var data = ids.Where(A => hasreserves.Contains(A.id)).Select(B => new ReportByFoodResponse()
                {
                    foodId = B.id,
                    food = B.title,
                    items = reserves.Where(A => A.foodId == B.id).ToList()
                });

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        ///  گزارش بر اساس غذا
        /// </summary>
        [Route("reportByFood")]
        [HttpGet]
        public async Task<IActionResult> ReportByFood([FromQuery] DateTime? fromDate, DateTime? toDate, string search, int? foodType, int? foodGroupId, int? customerId, int? customerParentId, int? branchId)
        {
            try
            {
                int i = 0;
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.vReserves
                                         .Where(c => c.Num > 0 && c.State != (int)EnumReserveState.perdict)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             state = A.State,
                                             userId = A.UserId,

                                             //userDescription = A.User.Description,
                                             customer = A.CustomerTitle,
                                             customerId = A.CustomerId,
                                             foodId = A.FoodId,
                                             food = A.FoodTitle,
                                             foodGroupId = A.FoodGroupId ?? 0,
                                             foodGroup = A.FoodGroupTitle,
                                             customerParent = A.CustomerParentTitle,
                                             customerParentId = A.CustomerParentId,
                                             foodType = A.FoodType,
                                             qty = A.Num,
                                             mealType = A.MealType,
                                             branchId = A.BranchId,
                                             isFood = A.IsFood
                                         });

                if (fromDate is not null)
                    Q = Q.Where(A => A.dateTime.Date >= fromDate.Value.Date);
                if (toDate is not null)
                    Q = Q.Where(A => A.dateTime.Date <= toDate.Value.Date);
                if (search is not null)
                    Q = Q.Where(A => A.food.Contains(search) || A.foodGroup.Contains(search) || A.customer.Contains(search) || A.customerParent.Contains(search));
                if (foodGroupId is not null)
                    Q = Q.Where(A => A.foodGroupId == foodGroupId);
                if (customerId is not null)
                    Q = Q.Where(A => A.customerId == customerId);
                if (customerParentId is not null)
                    Q = Q.Where(A => A.customerParentId == customerParentId);
                if (branchId is not null)
                    Q = Q.Where(A => A.branchId == branchId);
                if (foodType is not null)
                    Q = Q.Where(A => A.foodType == foodType);

                var reserves = await Q.ToListAsync();

                var foodreserves = reserves.Select(A => A.foodId).Distinct().ToList();
                var customerreserves = reserves.Select(A => A.customerId).Distinct().ToList();

                var foods = await _NarijeDBContext.Foods.Where(A => foodreserves.Contains(A.Id)).ToListAsync();
                var meals = await _NarijeDBContext.Meal.ToListAsync();
                var customers = await _NarijeDBContext.Customers.Where(A => customerreserves.Contains(A.Id)).ToListAsync();

                List<string> header = new();
                header.Add("نام شرکت");
                header.AddRange(foods.Select(A => A.Title));
                header.AddRange(meals.Select(A => A.Title));
                header.Add("جمع کل");


                List<List<string>> body = new();
                foreach (var customer in customers)
                {
                    List<string> items = new();
                    var customerParentTitle = await _NarijeDBContext.Customers.Where(c => c.Id == customer.ParentId).Select(c => c.Title).FirstOrDefaultAsync();
                    var compeleteCompanyName = $"{customerParentTitle} - {customer.Title}";
                    items.Add(compeleteCompanyName);
                    foreach (var food in foods)
                    {
                        i = reserves.Where(A => A.foodId == food.Id && A.customerId == customer.Id).Sum(A => A.qty);
                        items.Add(i.ToString());
                    }
                    foreach (var meal in meals)
                    {
                        var sumOfAll = reserves.Where(a => a.mealType == meal.Id && a.customerId == customer.Id && a.isFood == true).Sum(A => A.qty);
                        items.Add(sumOfAll.ToString());
                    }
                    body.Add(items);
                    i = reserves.Where(A => A.customerId == customer.Id && A.isFood == true).Sum(A => A.qty);
                    items.Add(i.ToString());

                }




                List<string> sum = new();

                foreach (var food in foods)
                {
                    i = reserves.Where(A => A.foodId == food.Id).Sum(A => A.qty);
                    sum.Add(i.ToString());
                }
                i = reserves.Sum(A => A.qty);
                sum.Add(i.ToString());

                var result = new
                {
                    header = header,
                    body = body,
                    sum = sum,

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

        #region Export ------------------------------------------------------------
        /// <summary>
        ///  خروجی شماره 1
        /// </summary>
        [Route("exportFoods")]
        [HttpGet]
        public async Task<IActionResult> ExportFoods([FromQuery] List<int> foodIds)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Foods
                                        .Select(F => new
                                        {
                                            id = F.Id,
                                            title = F.Title,
                                            group = F.Group.Title,
                                            active = F.Active ? "فعال" : "غیر فعال",
                                            guest = F.IsGuest ? "بلی" : "خیر",
                                            hasType = F.HasType ? "بلی" : "خیر"
                                        });

                if ((foodIds != null) && foodIds.Count > 0)
                {
                    Q = Q.Where(A => foodIds.Contains(A.id));
                }

                var data = await Q.ToListAsync();

                var body = data.Select(A => new List<string>
                {
                    A.id.ToString(),
                    A.title,
                    A.group,
                    A.active,
                    A.guest,
                    A.hasType
                });

                var result = new
                {
                    header = new List<string>
                    {
                        "آی دی",
                        "نام",
                        "گروه کالا",
                        "فعال/غیرفعال",
                        "غذای مهمان",
                        "نوع دارد"
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
        ///  خروجی شماره 1
        /// </summary>
        [Route("exportPrice")]
        [HttpGet]
        public async Task<IActionResult> ExportPrice([FromQuery] int customerId, [FromQuery] string food, [FromQuery] string foodGroup)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var customer = await _NarijeDBContext.Customers.Where(A => A.Id == customerId).FirstOrDefaultAsync();


                var Q = _NarijeDBContext.Foods
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
                                            groupId = A.GroupId,
                                            group = A.Group.Title,
                                            hasType = A.HasType,
                                            customer = customer.Title,
                                            echoPrice = A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id) == null ? 0 : A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id).Select(B => B.EchoPrice).FirstOrDefault(),
                                            specialPrice = A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id) == null ? 0 : A.FoodPrices.Where(B => B.CustomerId == customerId && B.FoodId == A.Id).Select(B => B.SpecialPrice).FirstOrDefault(),
                                        });


                if (food != null)
                    Q = Q.Where(A => A.title.Contains(food));
                if (foodGroup != null)
                    Q = Q.Where(A => A.group.Contains(foodGroup));

                var data = await Q.ToListAsync();

                var body = data.Select(A => new List<string>
                {
                    A.id.ToString(),
                    A.title,
                    A.group,
                    A.echoPrice.ToString(),
                    A.specialPrice.ToString(),
                    A.customer
                });

                var result = new
                {
                    header = new List<string>
                    {
                        "آی دی",
                        "نام",
                        "گروه کالا",
                        "قیمت اکو",
                        "قیمت ویژه",
                        "نام شرکت",
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
        ///  خروجی شماره 3
        /// </summary>
        [Route("exportCustomers")]
        [HttpGet]
        public async Task<IActionResult> ExportCustomers([FromQuery] List<int> ids)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Customers
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            title = A.Title,
                                            tel = A.Tel,
                                            contractStartDate = A.ContractStartDate,
                                            address = A.Address,
                                            cancelPercent = A.CancelPercent,
                                            cancelPercentPeriod = A.CancelPercentPeriod,
                                            cancelTime = A.CancelTime,
                                            guestTime = A.GuestTime,
                                            reserveTime = A.ReserveTime,
                                            reserveAfter = A.ReserveAfter,
                                            reserveTo = A.ReserveTo,
                                            active = A.Active ? "فعال" : "غیرفعال",
                                            foodType = EnumHelper<EnumFoodType>.GetDisplayValue((EnumFoodType)A.FoodType),
                                            showPrice = A.ShowPrice ? "بلی" : "خیر"
                                        })
                                        .AsNoTracking();

                if ((ids != null) && ids.Count > 0)
                {
                    Q = Q.Where(A => ids.Contains(A.id));
                }

                var data = await Q.ToListAsync();

                var body = data.Select(A => new List<string>
                {
                    A.id.ToString(),
                    A.title,
                    A.tel,
                    PersianDateTimeUtils.ToPersianDateTimeString(A.contractStartDate, "yyyy-MM-dd"),
                    A.address,
                    A.cancelPercent.ToString(),
                    A.cancelPercentPeriod.ToString(),
                    A.cancelTime.ToString(),
                    A.guestTime.ToString(),
                    A.reserveTime.ToString(),
                    A.reserveAfter.ToString(),
                    A.reserveTo.ToString(),
                    A.active,
                    A.foodType,
                    A.showPrice,
                });

                var result = new
                {
                    header = new List<string>
                    {
                        "آی دی",
                        "نام شرکت",
                        "تلفن",
                        "شروع قرارداد",
                        "آدرس",
                        "درصد مجاز کنسلی",
                        "بازه محاسبه کنسلی",
                        "مهلت مجاز کنسلی",
                        "مهلت مجاز رزرو میهمان",
                        "مهلت مجاز رزرو روز آینده",
                        "رزرو از روز",
                        "حداکثر روز قابل رزرو",
                        "وضعیت",
                        "نوی غذای پیش فرض",
                        "نمایش قیمت به پرسنل"
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
        ///  خروجی گزارش کلی
        ///  </summary>
        [Route("exportReserves")]
        [HttpGet]
        public async Task<IActionResult> ExportReserves([FromQuery] List<int> ids, DateTime? fromDate, DateTime? toDate, int? customerId)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Reserves
                                         .Select(A => new ExportResservesResponse()
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date.ToString(),
                                             dt = A.DateTime,
                                             customerId = A.CustomerId,
                                             userId = A.UserId,
                                             userName = A.User.Fname + " " + A.User.Lname,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             foodGroup = A.Food.Group.Title,
                                             foodType = EnumHelper<EnumFoodType>.GetDisplayValue((EnumFoodType)A.FoodType),
                                             price = A.Price,
                                             qty = A.Num
                                         });


                if ((ids != null) && ids.Count > 0)
                {
                    Q = Q.Where(A => ids.Contains(A.id));
                }
                if (fromDate is not null)
                {
                    Q = Q.Where(A => A.dt.Date >= fromDate.Value.Date);
                }
                if (toDate is not null)
                {
                    Q = Q.Where(A => A.dt.Date <= toDate.Value.Date);
                }
                if (customerId is not null)
                {
                    Q = Q.Where(A => A.customerId == customerId);
                }
                var data = await Q.ToListAsync();

                foreach (var item in data)
                {
                    item.dateTime = PersianDateTimeUtils.ToPersianDateTimeString(DateTime.Parse(item.dateTime), "yyyy-MM-dd");
                }


                var body = data.Select(A => new List<string>
                {
                    A.userId.ToString(),
                    A.dateTime.ToString(),
                    A.userName,
                    A.food,
                    A.foodGroup,
                    A.qty.ToString(),
                    A.price.ToString(),
                    A.foodType
                });

                var result = new
                {
                    header = new List<string>
                    {
                        "آی دی",
                        "تاریخ",
                        "نام کارمند",
                        "نام شرکت",
                        "نام غذا",
                        "گروه غذا",
                        "تعداد",
                        "قیمت",
                        "حجم غذا"
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
        ///  خروجی شماره 3
        /// </summary>
        [Route("exportMenu")]
        [HttpGet]
        public async Task<IActionResult> ExportMenu([FromQuery] int customerId, [FromQuery] DateTime? date)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Menus
                                    .Where(A => A.CustomerId == customerId)
                                    .Select(A => new ExportMenuResponse()
                                    {
                                        datetime = A.DateTime.Date.ToString(),
                                        dt = A.DateTime,
                                        maxReserve = A.MaxReserve,
                                        foodId = A.FoodId,
                                        food = A.Food.Title,
                                        foodGroupId = A.Food.GroupId,
                                        foodGroup = A.Food.Group.Title,
                                        foodType = EnumHelper<EnumFoodType>.GetDisplayValue((EnumFoodType)A.FoodType),
                                        customer = A.Customer.Title
                                    });

                if (date is not null)
                {
                    Q = Q.Where(A => A.dt.Date == date.Value.Date);
                }
                ;
                var data = await Q.ToListAsync();

                var body = data.Select(A => new List<string>
                {
                    A.foodId.ToString(),
                    PersianDateTimeUtils.ToPersianDateTimeString(DateTime.Parse(A.datetime), "yyyy-MM-dd"),
                    A.food,
                    A.foodGroup,
                    A.foodType,
                    A.maxReserve.ToString(),
                    A.customer
                });

                var result = new
                {
                    header = new List<string>
                    {
                        "آی دی",
                        "تاریخ",
                        "نام غذا",
                        "گروه کالا",
                        "حجم غذا",
                        "حداکثر رزرو غذا",
                        "نام شرکت"
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
        ///  خروجی کاربران
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
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             fName = A.Fname,
                                             lName = A.Lname,
                                             lastLogin = A.LastLogin,
                                             mobile = A.Mobile,
                                             role = EnumHelper<EnumRole>.GetDisplayValue((EnumRole)A.Role),
                                             customer = A.Customer == null ? "" : A.Customer.Title
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
                    PersianDateTimeUtils.ToPersianDateTimeString(A.lastLogin, "yyyy-MM-dd"),
                    A.role,
                    A.customer
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
                        "نقش",
                        "شرکت"
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
        [Route("export2")]
        [HttpGet]
        public async Task<IActionResult> Export2([FromQuery] DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Reserves
                                         .Select(A => new ReportResponse()
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             state = A.State,
                                             userId = A.UserId,
                                             userName = A.User.Fname + " " + A.User.Lname,
                                             //userDescription = A.User.Description,
                                             customer = A.Customer.Title,
                                             customerId = A.CustomerId,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             foodGroupId = A.Food.GroupId,
                                             foodGroup = A.Food.Group.Title,
                                             isFood = A.Food.IsFood,
                                             qty = A.Num
                                         });

                if (fromDate is not null)
                    Q = Q.Where(A => A.dateTime.Date >= fromDate.Value.Date);
                if (toDate is not null)
                    Q = Q.Where(A => A.dateTime.Date <= toDate.Value.Date);

                var reserves = await Q.ToListAsync();

                var foodreserves = reserves.Select(A => A.foodId).Distinct().ToList();
                var customerreserves = reserves.Select(A => A.customerId).Distinct().ToList();

                var foods = await _NarijeDBContext.Foods.Where(A => foodreserves.Contains(A.Id)).ToListAsync();
                var customers = await _NarijeDBContext.Customers.Where(A => customerreserves.Contains(A.Id)).ToListAsync();

                List<string> header = new();
                header.Add("نام شرکت");
                header.AddRange(foods.Select(A => A.Title));

                List<List<string>> body = new();
                foreach (var customer in customers)
                {
                    List<string> items = new();
                    items.Add(customer.Title);
                    foreach (var food in foods)
                    {
                        int i = reserves.Where(A => A.foodId == food.Id && A.customerId == customer.Id).Count();
                        items.Add(i.ToString());
                    }
                    body.Add(items);
                }

                List<string> sum = new();
                foreach (var food in foods)
                {
                    int i = reserves.Where(A => A.foodId == food.Id).Count();
                    sum.Add(i.ToString());
                }

                var result = new
                {
                    header = header,
                    body = body,
                    sum = sum
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
        ///  خروجی فاکتورها
        /// </summary>
        [Route("exportInvoices")]
        [HttpGet]
        public async Task<IActionResult> ExportInvoices()
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Q = _NarijeDBContext.Invoices;

                var data = await Q.ToListAsync();

                var body = data.Select(A => new List<string>
                {
                    A.Id.ToString(),
                    A.Serial,
                    PersianDateTimeUtils.ToPersianDateTimeString(A.DateTime, "yyyy-MM-dd"),
                    A.Customer.Title,
                    A.Customer.Tel,
                    A.Qty.ToString(),
                    (A.FinalPrice*10).ToString(),
                    A.PayType == 0 ? "نقدی" : "اعتباری",
                    (A.TransportFee*A.TransportQty*10).ToString(),
                    A.TransportQty.ToString(),
                    A.HasVat ? "بلی" : "خیر",
                    A.Description,
                    PersianDateTimeUtils.ToPersianDateTimeString(A.FromDate, "yyyy-MM-dd"),
                    PersianDateTimeUtils.ToPersianDateTimeString(A.ToDate, "yyyy-MM-dd")
                });

                var result = new
                {
                    header = new List<string>
                    {
                        "آی دی",
                        "سریال",
                        "تاریخ ثبت فاکتور",
                        "نام شرکت",
                        "شماره تماس",
                        "تعداد",
                        "قیمت",
                        "نقدی/غیرنقدی",
                        "هزینه حمل",
                        "تعداد حمل و نقل",
                        "اعمال ارزش افزوده",
                        "توضیحات",
                        "از تاریخ",
                        "تا تاریخ"
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

        #region Invoice -------------------------------------------------------------
        /// <summary>
        ///  اطلاعات فاکتور
        /// </summary>
        [Route("invoice")]
        [HttpGet]
        public async Task<IActionResult> Invoices([FromQuery] int id)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var invoice = await _NarijeDBContext.Invoices
                                        .Where(A => A.Id == id)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            serial = A.Serial,
                                            customerId = A.CustomerId,
                                            customer = A.Customer.Title,
                                            qty = A.Qty,
                                            totalPrice = A.TotalPrice * 10,
                                            hasVat = A.HasVat,
                                            vat = A.Vat * 10,
                                            finalPrice = A.FinalPrice * 10,
                                            description = A.Description,
                                            datetime = A.DateTime,
                                            fromDate = A.FromDate,
                                            toDate = A.ToDate,
                                            updatedAt = A.UpdatedAt,
                                            transportFee = A.TransportFee,
                                            payType = A.PayType,
                                            transportQty = A.TransportQty
                                        })
                                        .FirstOrDefaultAsync();

                var cusotmerIds = _NarijeDBContext.Customers
                                        .Where(A => A.Id == invoice.customerId || A.ParentId == invoice.customerId)
                                        .Select(A => A.Id)
                                        .ToList();
                bool WithCustomerName = false;
                if (cusotmerIds.Count > 1)
                    WithCustomerName = true;


                var D = _NarijeDBContext.Reserves
                                         .Where(A => cusotmerIds.Contains(A.CustomerId) && A.DateTime.Date >= invoice.fromDate.Date
                                                            && A.DateTime.Date <= invoice.toDate.Date && A.State != (int)EnumReserveState.canceled)
                                         .Select(A => A.DateTime.Date)
                                         .OrderBy(A => A.Date)
                                         .Distinct();


                var dates = await D.ToListAsync();

                var reserves = await _NarijeDBContext.Reserves
                                         .Where(A => cusotmerIds.Contains(A.CustomerId) && dates.Contains(A.DateTime.Date) && A.State != (int)EnumReserveState.canceled)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             customer = A.Customer.Title,
                                             customerId = A.CustomerId,
                                             state = A.State,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             price = A.Price,
                                             qty = A.Num,
                                             vat = A.Food.Vat
                                         }).ToListAsync();

                int row = 1;
                bool first = true;
                List<InvoiceDetailResponse> details = new();
                foreach (var date in dates)
                {
                    var foods = reserves
                        .Where(A => A.dateTime.Date == date)
                        .Select(A => new
                        {
                            A.foodId,
                            A.food,
                            A.customer,
                            A.customerId,
                            price = A.price * 10,
                            vat = A.vat
                        }).Distinct().ToList();
                    first = true;
                    foreach (var food in foods)
                    {
                        var qty = reserves.Where(A => A.dateTime.Date == date && A.foodId == food.foodId && A.customerId == food.customerId && A.price == food.price / 10).Sum(A => A.qty);
                        details.Add(new InvoiceDetailResponse()
                        {
                            row = row,
                            datetime = date.ToString("yyyy-MM-dd"),
                            foodId = food.foodId.ToString(),
                            food = food.food + (!WithCustomerName ? "" : $" ({food.customer})"),
                            price = food.price,
                            qty = qty,
                            type = 0,
                            totalPrice = qty * food.price,
                            vat = food.vat == null ? (long)((qty * food.price) * 0.09) : (long)((qty * food.price) * ((double)food.vat / 100)) //invoice.hasVat ? (long)((qty * food.price) * 0.09) : 0
                        });
                        first = false;
                        row++;
                    }

                }
                foreach (var item in details)
                    item.finalPrice = item.vat + item.totalPrice;

                var invoicedetails = await _NarijeDBContext.InvoiceDetails.Where(A => A.InvoiceId == invoice.id).ToListAsync();
                first = true;
                foreach (var item in invoicedetails)
                {
                    details.Add(new InvoiceDetailResponse()
                    {
                        row = row,
                        datetime = invoice.datetime.ToString("yyyy-MM-dd"),
                        foodId = item.FoodId.ToString(),
                        food = item.Food.Title,// + (!WithCustomerName ? "" : $" ({item.Invoice.Customer.Title})"),
                        price = item.Price * 10,
                        qty = item.Qty,
                        totalPrice = item.TotalPrice * 10,
                        type = 1,
                        vat = item.Vat * 10,
                        finalPrice = item.FinalPrice * 10
                    });
                    row++;
                    first = false;
                }

                row = 1;
                details = details.OrderBy(A => A.datetime).ToList();
                string lastdate = "";
                foreach (var item in details)
                {
                    item.row = row++;
                    if (lastdate == item.datetime)
                        item.datetime = "";
                    lastdate = item.datetime;

                }

                if (invoice.transportFee > 0)
                {
                    details.Add(new InvoiceDetailResponse()
                    {
                        row = row,
                        datetime = "",
                        foodId = "-",
                        food = "هزینه حمل",
                        price = invoice.transportFee * 10,
                        qty = invoice.transportQty,
                        totalPrice = (invoice.transportFee * invoice.transportQty) * 10,
                        type = 0,
                        vat = 0,
                        finalPrice = (invoice.transportFee * invoice.transportQty) * 10
                    });
                }

                var company = await _NarijeDBContext.Settings.Select(A => new
                {
                    companyName = A.CompanyName,
                    tel = A.Tel,
                    address = A.Address,
                    city = A.CityId == null ? "" : A.City.Title,
                    province = A.ProvinceId == null ? "" : A.Province.Title,
                    economicCode = A.EconomicCode,
                    regNumber = A.RegNumber,
                    nationalId = A.NationalId,
                    postalCode = A.PostalCode,
                    logo = A.CompanyGalleryId
                }).FirstOrDefaultAsync();

                var customer = await _NarijeDBContext.Customers
                    .Where(A => A.Id == invoice.customerId)
                    .Select(A => new
                    {
                        title = A.Title,
                        tel = A.Tel,
                        address = A.Address,
                        city = A.CityId == null ? "" : A.City.Title,
                        province = A.ProvinceId == null ? "" : A.Province.Title,
                        economicCode = A.EconomicCode,
                        regNumber = A.RegNumber,
                        nationalId = A.NationalId,
                        postalCode = A.PostalCode
                    }).FirstOrDefaultAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: new
                {
                    invoice,
                    details,
                    company,
                    customer
                }));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// لیست فاکتورها
        /// </summary>
        [Route("invoices")]
        [HttpGet]
        public async Task<IActionResult> Invoices([FromQuery] int? Page, [FromQuery] int? Limit, [FromQuery] string search,
                                                        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int? customerId)
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

                var Q = _NarijeDBContext.Invoices
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            serial = A.Serial,
                                            customerId = A.CustomerId,
                                            customer = A.Customer.Title,
                                            qty = A.Qty,
                                            totalPrice = A.TotalPrice,
                                            hasVat = A.HasVat,
                                            vat = A.Vat,
                                            finalPrice = A.FinalPrice,
                                            description = A.Description,
                                            datetime = A.DateTime,
                                            fromDate = A.FromDate,
                                            toDate = A.ToDate,
                                            updatedAt = A.UpdatedAt,
                                            transportFee = A.TransportFee,
                                            payType = A.PayType
                                        })
                                        .AsNoTracking();

                if (customerId != null)
                {
                    Q = Q.Where(A => A.customerId == customerId);
                }
                if (fromDate is not null)
                {
                    Q = Q.Where(A => A.fromDate.Date >= fromDate.Value.Date);
                }
                if (toDate is not null)
                {
                    Q = Q.Where(A => A.toDate.Date <= toDate.Value.Date);
                }

                if (search != null)
                    Q = Q.Where(A => A.serial.Contains(search));

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
        /// ویرایش فاکتور
        /// </summary>
        [Route("invoice")]
        [HttpPut]
        public async Task<IActionResult> EditInvoice([FromBody] InvoiceRequest invoice)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var inv = await _NarijeDBContext.Invoices.Where(A => A.Id == invoice.id)
                                        .FirstOrDefaultAsync();
                if (inv is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "فاکتور یافت نشد"));

                DateTime LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

                inv.Serial = invoice.serial;
                inv.Description = invoice.description;
                inv.HasVat = invoice.hasVat;
                inv.FromDate = invoice.fromDate;
                inv.ToDate = invoice.toDate;
                inv.Serial = invoice.serial;
                inv.UpdatedAt = LocalDate;
                inv.TransportFee = invoice.transportFee;
                inv.PayType = invoice.payType;
                inv.TransportQty = invoice.transportQty;
                inv.TotalPrice = 0;
                inv.Vat = 0;
                inv.FinalPrice = 0;
                inv.Qty = 0;

                var cusotmerIds = _NarijeDBContext.Customers
                                        .Where(A => A.Id == inv.CustomerId || A.ParentId == inv.CustomerId)
                                        .Select(A => A.Id)
                                        .ToList();

                var MustDelete = inv.InvoiceDetails.ToList();
                _NarijeDBContext.InvoiceDetails.RemoveRange(MustDelete);

                var foods = await _NarijeDBContext.Foods.Select(A => new
                {
                    Id = A.Id,
                    A.Vat,
                    Price = A.FoodPrices.Where(B => B.CustomerId == inv.CustomerId).Select(A => A.EchoPrice).FirstOrDefault() == 0 ?
                            A.EchoPrice : A.FoodPrices.Where(B => B.CustomerId == inv.CustomerId).Select(A => A.EchoPrice).FirstOrDefault()
                }).ToListAsync();

                List<InvoiceDetail> details = new();
                foreach (var item in invoice.details)
                {
                    var food = foods.Where(A => A.Id == item.foodId).FirstOrDefault();

                    if (food != null)
                    {
                        var detail = new InvoiceDetail()
                        {
                            Invoice = inv,
                            FoodId = item.foodId,
                            Qty = item.qty,
                            Price = food.Price,
                            TotalPrice = food.Price * item.qty,
                            Vat = 0
                        };

                        if (food.Vat == null)
                            detail.Vat = (int)(detail.TotalPrice * 0.09);
                        else
                            detail.Vat = (int)(detail.TotalPrice * ((double)food.Vat / 100));

                        //if (inv.HasVat)
                        //    detail.Vat = (int)(detail.TotalPrice * 0.09);
                        detail.FinalPrice = detail.Vat + detail.TotalPrice;

                        details.Add(detail);
                    }
                }

                foreach (var item in details)
                {
                    inv.Qty += item.Qty;
                    inv.Vat += item.Vat;
                    inv.TotalPrice += item.TotalPrice;
                    inv.FinalPrice += item.FinalPrice;
                }

                var reserve = await _NarijeDBContext.Reserves
                                         .Where(A => cusotmerIds.Contains(A.CustomerId) &&
                                                        A.DateTime.Date >= inv.FromDate.Date && A.DateTime.Date <= inv.ToDate.Date &&
                                                        A.State != (int)EnumReserveState.canceled)
                                         .Select(A => new
                                         {
                                             foodId = A.FoodId,
                                             qty = A.Num,
                                             price = A.Price,
                                             total = A.Num * A.Price,
                                             vat = A.Food.Vat
                                         }).ToListAsync();
                long price = 0;
                int qty = 0;
                long vat = 0;
                foreach (var item in reserve)
                {
                    price += item.total;
                    qty += item.qty;
                    vat += item.vat == null ? (long)(item.total * 0.09) : (long)(item.total * ((double)item.vat / 100));
                }
                inv.Qty += qty;
                inv.TotalPrice += price;

                //var vat = (long)(price * 0.09);
                //if (!inv.HasVat)
                //    vat = 0;
                inv.Vat += vat;
                inv.FinalPrice += price + vat;


                if (inv.TransportFee > 0)
                {
                    inv.TotalPrice += inv.TransportFee * inv.TransportQty;
                    inv.FinalPrice += inv.TransportFee * inv.TransportQty;
                }

                _NarijeDBContext.Invoices.Update(inv);
                await _NarijeDBContext.InvoiceDetails.AddRangeAsync(details);
                await _NarijeDBContext.SaveChangesAsync();

                var result = await _NarijeDBContext.Invoices
                                        .Where(A => A.Id == inv.Id)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            serial = A.Serial,
                                            customerId = A.CustomerId,
                                            customer = A.Customer.Title,
                                            qty = A.Qty,
                                            totalPrice = A.TotalPrice,
                                            hasVat = A.HasVat,
                                            vat = A.Vat,
                                            finalPrice = A.FinalPrice,
                                            description = A.Description,
                                            datetime = A.DateTime,
                                            fromDate = A.FromDate,
                                            toDate = A.ToDate,
                                            updatedAt = A.UpdatedAt,
                                            transportFee = A.TransportFee,
                                            payType = A.PayType
                                        })
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// افزودن فاکتور
        /// </summary>
        [Route("invoice")]
        [HttpPost]
        public async Task<IActionResult> AddInvoice([FromBody] InvoiceRequest invoice)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var customer = await _NarijeDBContext.Customers.Where(A => A.Id == invoice.customerId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (customer is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "مشتری یافت نشد"));

                DateTime LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

                var inv = new Invoice()
                {
                    Description = invoice.description,
                    Serial = invoice.serial,
                    DateTime = LocalDate,
                    UpdatedAt = LocalDate,
                    CustomerId = invoice.customerId,
                    HasVat = invoice.hasVat,
                    FinalPrice = 0,
                    FromDate = invoice.fromDate,
                    ToDate = invoice.toDate,
                    TransportQty = invoice.transportQty,
                    Qty = 0,
                    Vat = 0,
                    TotalPrice = 0,
                    TransportFee = invoice.transportFee,
                    PayType = invoice.payType
                };

                var cusotmerIds = _NarijeDBContext.Customers
                                        .Where(A => A.Id == inv.CustomerId || A.ParentId == inv.CustomerId)
                                        .Select(A => A.Id)
                                        .ToList();

                var foods = await _NarijeDBContext.Foods.Select(A => new
                {
                    Id = A.Id,
                    A.Vat,
                    Price = A.FoodPrices.Where(B => B.CustomerId == inv.CustomerId).Select(A => A.EchoPrice).FirstOrDefault() == 0 ?
                            A.EchoPrice : A.FoodPrices.Where(B => B.CustomerId == inv.CustomerId).Select(A => A.EchoPrice).FirstOrDefault()
                }).ToListAsync();

                List<InvoiceDetail> details = new();
                foreach (var item in invoice.details)
                {
                    var food = foods.Where(A => A.Id == item.foodId).FirstOrDefault();

                    if (food != null)
                    {
                        var detail = new InvoiceDetail()
                        {
                            Invoice = inv,
                            FoodId = item.foodId,
                            Qty = item.qty,
                            Price = food.Price,
                            TotalPrice = (food.Price) * item.qty,
                            Vat = 0
                        };
                        if (food.Vat == null)
                            detail.Vat = (int)(detail.TotalPrice * 0.09);
                        else
                            detail.Vat = (int)(detail.TotalPrice * ((double)food.Vat / 100));

                        //if (inv.HasVat)
                        //    detail.Vat = (int)(detail.TotalPrice * 0.09);
                        detail.FinalPrice = detail.Vat + detail.TotalPrice;

                        details.Add(detail);
                    }
                }

                foreach (var item in details)
                {
                    inv.Qty += item.Qty;
                    inv.Vat += item.Vat;
                    inv.TotalPrice += item.TotalPrice;
                    inv.FinalPrice += item.FinalPrice;
                }

                var reserve = await _NarijeDBContext.Reserves
                                         .Where(A => cusotmerIds.Contains(A.CustomerId) &&
                                                        A.DateTime.Date >= inv.FromDate.Date && A.DateTime.Date <= inv.ToDate.Date &&
                                                        A.State != (int)EnumReserveState.canceled)
                                         .Select(A => new
                                         {
                                             foodId = A.FoodId,
                                             qty = A.Num,
                                             price = A.Price,
                                             total = A.Num * A.Price,
                                             vat = A.Food.Vat
                                         }).ToListAsync();
                long price = 0;
                int qty = 0;
                long vat = 0;
                foreach (var item in reserve)
                {
                    price += item.total;
                    qty += item.qty;
                    vat += item.vat == null ? (long)(item.total * 0.09) : (long)(item.total * ((double)item.vat / 100));
                }
                inv.Qty += qty;
                inv.TotalPrice += price;
                //var vat = (long)(price * 0.09);
                //if (!inv.HasVat)
                //    vat = 0;
                inv.Vat += vat;
                inv.FinalPrice += price + vat;

                if (inv.TransportFee > 0)
                {
                    inv.TotalPrice += inv.TransportFee * inv.TransportQty;
                    inv.FinalPrice += inv.TransportFee * inv.TransportQty;
                }

                await _NarijeDBContext.Invoices.AddAsync(inv);
                await _NarijeDBContext.InvoiceDetails.AddRangeAsync(details);
                await _NarijeDBContext.SaveChangesAsync();

                var result = await _NarijeDBContext.Invoices
                                        .Where(A => A.Id == inv.Id)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            serial = A.Serial,
                                            customerId = A.CustomerId,
                                            customer = A.Customer.Title,
                                            qty = A.Qty,
                                            totalPrice = A.TotalPrice,
                                            hasVat = A.HasVat,
                                            vat = A.Vat,
                                            finalPrice = A.FinalPrice,
                                            description = A.Description,
                                            datetime = A.DateTime,
                                            fromDate = A.FromDate,
                                            toDate = A.ToDate,
                                            updatedAt = A.UpdatedAt,
                                            transportFee = A.TransportFee,
                                            payType = A.PayType
                                        })
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: result));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        #region AdminReserve -------------------------------------------------------------
        /// <summary>
        /// مشاهده رزور ادمین
        /// </summary>
        [Route("adminReserves")]
        [HttpGet]
        public async Task<IActionResult> AdminReserves([FromQuery] DateTime datetime, int customerId, string search, int? groupId, int? state, int pageNumber = 1, int pageSize = 30, int mealId = 0, bool fromMenu = true)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "Access expired"));

                var stateValue = state.HasValue ? state.Value : (int)EnumReserveState.admin;
                var Customer = await _NarijeDBContext.Customers
                                      .Where(A => A.Id == customerId)
                                      .AsNoTracking()
                                      .FirstOrDefaultAsync();

                if (Customer is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "Customer not found"));

                var UserReserves = await _NarijeDBContext.Reserves
                                      .Where(A => A.CustomerId == Customer.Id && A.DateTime.Date == datetime.Date && A.State == stateValue && A.MealType == mealId)
                                     .Include(r => r.Food)
                                       .ThenInclude(f => f.Group)
                                    .Include(r => r.Food)
                                        .ThenInclude(f => f.Gallery)
                                      .AsNoTracking()
                                      .ToListAsync();

                int ftype = Customer.FoodType;

                List<ReserveResponse> responses;

                var branch = await _NarijeDBContext.Customers
                                       .Where(c => c.Id == customerId)
                                       .Select(c => new { Id = c.Id, ParentId = c.ParentId })
                                       .FirstOrDefaultAsync();

                var customerEntity = await _NarijeDBContext.Customers
                                            .Where(c => c.Id == branch.ParentId)
                                            .FirstOrDefaultAsync();

                if (fromMenu)
                {
                    var persianCalendar = new PersianCalendar();
                    int year = persianCalendar.GetYear(datetime.Date);
                    int month = persianCalendar.GetMonth(datetime.Date);


                    var menuInfo = await _NarijeDBContext.CustomerMenuInfo
                                        .Where(c => c.CustomerId == customerEntity.Id && c.Month == month && c.Year == year)
                                        .Select(c => new { Id = c.MenuInfoId })
                                        .FirstOrDefaultAsync();


                    var priceHelper = new PriceHelper();

                    var menus = await _NarijeDBContext.Menus
                                    .Where(A => A.MenuInfoId == menuInfo.Id && A.DateTime.Date == datetime.Date && A.MealType == mealId)
                                    .ToListAsync();

                    responses = (await Task.WhenAll(menus.Select(async A => new ReserveResponse
                    {
                        id = A.Id,
                        maxReserve = 10000,
                        foodId = A.FoodId,
                        food = A.Food.Title,
                        foodDescription = A.Food.Description,
                        foodGroupId = A.Food.GroupId,
                        foodType = ftype,
                        foodGroup = A.Food.Group.Title,
                        image = A.Food.Gallery == null ? "" : $"{A.Food.GalleryId}",
                        state = "",
                        price = await priceHelper.GetPriceForMenu(customerEntity, A, A.Food.Vip),
                        echoPrice = A.EchoPrice ?? A.Food.EchoPrice,
                        specialPrice = A.SpecialPrice ?? A.Food.SpecialPrice,
                        qty = 0,
                        fromMenu = true
                    }))).ToList();
                }
                else
                {
                    var priceHelper = new PriceHelper();

                    var foods = await _NarijeDBContext.Foods.ToListAsync();

                    responses = (await Task.WhenAll(foods.Select(async A => new ReserveResponse
                    {
                        id = A.Id,
                        maxReserve = 10000,
                        foodId = A.Id,
                        food = A.Title,
                        foodDescription = A.Description,
                        foodGroupId = A.GroupId,
                        foodGroup = A.Group.Title,
                        foodType = ftype,
                        image = A.Gallery == null ? "" : $"{A.GalleryId}",
                        state = "",
                        price = await priceHelper.GetPriceForMenu(customerEntity, A, A.Vip),
                        echoPrice = A.EchoPrice,
                        specialPrice = A.SpecialPrice,
                        qty = 0,
                        fromMenu = false
                    }))).ToList();
                }

                if (groupId != null)
                    responses = responses.Where(A => A.foodGroupId == groupId).ToList();
                if (search != null)
                    responses = responses.Where(A => A.food.Contains(search)).ToList();

                foreach (var item in responses)
                {
                    var r = UserReserves
                                .Where(A => A.DateTime.Date == datetime.Date && A.FoodId == item.foodId)
                                .FirstOrDefault();
                    if (r != null)
                    {
                        item.state = EnumHelper<EnumReserveState>.GetDisplayValue((EnumReserveState)r.State);
                        item.qty = r.Num;
                        item.price = r.Price;
                        item.foodType = r.FoodType;
                    }
                }
                var missingReserves = UserReserves
                       .Where(r => !responses.Any(x => x.foodId == r.FoodId))
                       .Select(r => new ReserveResponse
                       {
                           id = r.Id,
                           maxReserve = 10000,
                           foodId = r.FoodId,
                           food = r.Food.Title,
                           foodDescription = r.Food.Description,
                           foodGroupId = r.Food.GroupId,
                           foodGroup = r.Food.Group?.Title,
                           foodType = r.FoodType,
                           image = r.Food.Gallery == null ? "" : $"{r.Food.GalleryId}",
                           state = EnumHelper<EnumReserveState>.GetDisplayValue((EnumReserveState)r.State),
                           price = r.Price,
                           echoPrice = r.Price ,
                           specialPrice = r.Price ,
                           qty = r.Num,
                           fromMenu = fromMenu
                       }).ToList();

                if (groupId != null)
                    missingReserves = missingReserves.Where(x => x.foodGroupId == groupId).ToList();
                if (!string.IsNullOrWhiteSpace(search))
                    missingReserves = missingReserves.Where(x => x.food != null && x.food.Contains(search)).ToList();

                responses.InsertRange(0, missingReserves);

                var reservedFoodSet = new HashSet<int>(UserReserves.Select(r => r.FoodId));
                responses = responses
                    .OrderByDescending(x => reservedFoodSet.Contains(x.foodId)) 
                    .ThenBy(x => x.food)
                    .ToList();

                var totalCount = responses.Count;
                var pagedItems = responses.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var metaResult = new MetaResult
                {
                    Total = totalCount,
                    TotalInPage = pagedItems.Count,
                    TotalPage = totalPages,
                    CurrentPage = pageNumber,
                    Limit = pageSize,
                    Next = pageNumber < totalPages ? pageNumber + 1 : (int?)null,
                    Prev = pageNumber > 1 ? pageNumber - 1 : (int?)null
                };

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: pagedItems, _Meta: metaResult));
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }


        /// <summary>
        /// ویرایش رزرو ادمین
        /// </summary>
        //[ValidateAntiForgeryToken]
        [Route("adminReserves")]
        [HttpPut]
        public async Task<IActionResult> EditGuestReserves([FromBody] AdminReserveRequest day)
        {
            try
            {
                var Admin = await CheckAccess();
                if (Admin is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                var Customer = await _NarijeDBContext.Customers
                                         .Where(A => A.Id == day.customerId)
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync();



                if (Customer is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "اطلاعات شعبه یافت نشد"));

                var CustomerParent = await _NarijeDBContext.Customers
                                    .Where(A => A.Id == Customer.ParentId)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();

                if (CustomerParent is null)
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "اطلاعات شرکت یافت نشد"));

                var CustomerReserveTime = await _NarijeDBContext.CompanyMeal.Where(c => c.CustomerId == Customer.Id && c.MealId == day.mealId).FirstOrDefaultAsync();

                if (CustomerReserveTime == null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "وعده غذایی مورد نظر برای این شرکت تعریف نشده است"));

                }

                var UserReserves = await _NarijeDBContext.Reserves
                                    .Where(A => A.CustomerId == day.customerId && A.DateTime.Date == day.datetime.Date && A.State == day.reserveType && A.MealType == day.mealId)
                                    .ToListAsync();



                var tehranTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
                var tehranTime = TimeZoneInfo.ConvertTime(day.datetime, tehranTimeZone);

                var persianCalendar = new System.Globalization.PersianCalendar();
                var shamsiDayOfWeek = persianCalendar.GetDayOfWeek(tehranTime);

                int selectedBranch = 0;

                switch (shamsiDayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        selectedBranch = Customer.BranchForSaturday ?? 0;
                        break;
                    case DayOfWeek.Sunday:
                        selectedBranch = Customer.BranchForSunday ?? 0;
                        break;
                    case DayOfWeek.Monday:
                        selectedBranch = Customer.BranchForMonday ?? 0;
                        break;
                    case DayOfWeek.Tuesday:
                        selectedBranch = Customer.BranchForTuesday ?? 0;
                        break;
                    case DayOfWeek.Wednesday:
                        selectedBranch = Customer.BranchForWednesday ?? 0;
                        break;
                    case DayOfWeek.Thursday:
                        selectedBranch = Customer.BranchForThursday ?? 0;
                        break;
                    case DayOfWeek.Friday:
                        selectedBranch = Customer.BranchForFriday ?? 0;
                        break;

                    default:
                        selectedBranch = Customer.BranchForFriday ?? 0;
                        break;
                }

                if (selectedBranch == 0)
                {
                    return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "ابتدا شرکت خدمات دهنده را از تنظیمات شرکت برای این شعبه مشخص کنید"));
                }

                var foods = await _NarijeDBContext.Foods
                                    .Select(A => new
                                    {
                                        Id = A.Id,
                                        HasType = A.HasType,
                                        EchoPrice = A.EchoPrice,
                                        SpecialPrice = A.SpecialPrice,
                                        EchoPriceDefault = A.EchoPrice,
                                        SpecialPriceDefault = A.SpecialPrice,
                                        isFood = A.IsFood
                                    })
                                    .ToListAsync();

                var fids = foods.Select(A => A.Id).ToList();
                var Foods = _NarijeDBContext.Foods.Where(A => fids.Contains(A.Id)).ToList();



                foreach (var item in day.reserves)
                {
                    var r = UserReserves
                            .Where(A => A.FoodId == item.foodId)
                            .FirstOrDefault();

                    var food = foods.Where(A => A.Id == item.foodId).FirstOrDefault();
                    if (food is null)
                        continue;


                    if (r == null)
                    {
                        Reserve res = new()
                        {
                            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                            CustomerId = Customer.Id,
                            Num = item.qty,
                            UserId = Admin.Id,
                            DateTime = day.datetime.Date,
                            ReserveType = 0,
                            State = (int)(day.reserveType.HasValue ? day.reserveType : (int)EnumReserveState.admin),
                            FoodId = item.foodId,
                            FoodType = item.foodType,
                            Price = item.price ?? 0,
                            MealType = day.mealId ?? 0,
                            BranchId = selectedBranch,
                            PriceType = CustomerParent.PriceType,
                            DeliverHour = CustomerReserveTime.DeliverHour
                        };
                        await _NarijeDBContext.Reserves.AddAsync(res);
                    }
                    else
                    {
                        r.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                        r.Num = item.qty;
                        r.State = (int)(day.reserveType.HasValue ? day.reserveType : (int)EnumReserveState.admin);
                        r.FoodType = item.foodType;
                        r.Price = item.price ?? 0;
                        r.BranchId = selectedBranch;
                        _NarijeDBContext.Reserves.Update(r);
                    }
                }

                var ids = day.reserves.Select(A => A.foodId).ToList();

                var mustdeleted = UserReserves.Where(A => !ids.Contains(A.FoodId)).ToList();
                _NarijeDBContext.Reserves.RemoveRange(mustdeleted);

                var result = await _NarijeDBContext.SaveChangesAsync();

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: null));
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