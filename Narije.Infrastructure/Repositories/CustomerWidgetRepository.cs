using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.FoodGroup;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Narije.Core.DTOs.ViewModels.CustomerMenuInfo;
using System.Security.Claims;
namespace Narije.Infrastructure.Repositories
{
    public class CustomerWidgetRepository : BaseRepository<Customer>, ICustomerWidget
    {
        public CustomerWidgetRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
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


        public async Task<ApiResponse> GetSummary()
        {
            try
            {
                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "ICustomerWidget");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);
                var user = await CheckAccess();
                var customerId = user.CustomerId;

                if (customerId == null)
                {
                    return new ApiErrorResponse(
                        _Code: StatusCodes.Status400BadRequest,
                        _Message: "شرکتی به این کاربر انتساب داده نشده "
                    );
                }

                var reservations = await _NarijeDBContext.vReserves
                    .Where(r => r.CustomerId == customerId && r.Num > 0 && r.IsFood == true)
                    .ToListAsync();

                if (!reservations.Any())
                {
                    return new ApiOkResponse(
                        _Message: "SUCCESS",
                        _Data: new { Message = "رزروی برای این شرکت یافت نشد" },
                        _Header: header
                    );
                }

                var firstReserve = reservations.Min(r => r.DateTime);

                var totalReservations = reservations.Sum(r => r.Num);

                var totalPrice = reservations.Sum(r => r.Price * r.Num);

                var lastReserve = reservations.Max(r => r.DateTime);

                var favoriteFood = reservations
                    .GroupBy(r => new { r.FoodId, r.FoodTitle })
                    .OrderByDescending(g => g.Sum(r => r.Num))
                    .Select(g => new
                    {
                        g.Key.FoodId,
                        g.Key.FoodTitle,
                        Count = g.Sum(r => r.Num)
                    })
                    .FirstOrDefault();

                var mealWiseReservations = reservations
                    .GroupBy(r => new { r.MealType, r.MealTitle , r.MealImage })
                    .Select(g => new
                    {
                        mealId = g.Key.MealType,
                        mealTitle = g.Key.MealTitle,
                        MealImage = g.Key.MealImage,
                        count = g.Sum(r => r.Num)
                    })
                    .ToList();
                var numberOfUsers = await _NarijeDBContext.Users.Where(c => c.CustomerId == customerId).CountAsync();
                var result = new
                {
                    firstReserve = firstReserve,
                    totalReservations = totalReservations,
                    totalPrice = totalPrice,
                    lastReserve = lastReserve,
                    favoriteFood = favoriteFood,
                    mealWiseReservations = mealWiseReservations,
                    numberOfUsers = numberOfUsers
                };

                return new ApiOkResponse(
                    _Message: "SUCCESS",
                    _Data: result,
                    _Header: header
                );
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status500InternalServerError,
                    _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message
                );
            }
        }


        public async Task<ApiResponse> GetReserves(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "ReserveReport");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);
                var user = await CheckAccess();
                var customerId = user.CustomerId;

                if (customerId == null)
                {
                    return new ApiErrorResponse(
                        _Code: StatusCodes.Status400BadRequest,
                        _Message: "شرکتی به این کاربر انتساب داده نشده"
                    );
                }

                var reservations = await _NarijeDBContext.vReserves
                    .Where(r => r.CustomerId == customerId && r.DateTime >= fromDate && r.DateTime <= toDate)
                    .ToListAsync();

                if (!reservations.Any())
                {
                    return new ApiOkResponse(
                        _Message: "SUCCESS",
                        _Data: new { Message = "در این بازه سفارشی موجود نیست" },
                        _Header: header
                    );
                }

                var mealWiseReservations = reservations
                    .GroupBy(r => new { r.MealType, r.MealTitle,r.MealImage })
                    .Select(g => new
                    {
                        mealId = g.Key.MealType,
                        mealTitle = g.Key.MealTitle,
                        mealImage = g.Key.MealImage,
                        Foods = g.GroupBy(f => new { f.FoodId, f.FoodTitle , f.FoodGalleryId })
                                 .Select(fg => new
                                 {
                                     foodId = fg.Key.FoodId,
                                     foodTitle = fg.Key.FoodTitle,
                                     count = fg.Sum(r => r.Num),
                                     foodImage = fg.Key.FoodGalleryId
                                 })
                                 .OrderByDescending(f => f.count)
                                 .ToList()
                    })
                    .OrderBy(m => m.mealId) 
                    .ToList();

                return new ApiOkResponse(
                    _Message: "SUCCESS",
                    _Data: mealWiseReservations,
                    _Header: header
                );
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status500InternalServerError,
                    _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message
                );
            }
        }



    }
}
