using AutoMapper;
using AutoMapper.QueryableExtensions;
using Castle.Core.Resource;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Credit;
using Narije.Core.DTOs.ViewModels.Food;
using Narije.Core.DTOs.ViewModels.User;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Infrastructure.Repositories
{
    public class CreditRepository : BaseRepository<Credit>, ICreditRepository
    {
        private readonly IWalletRepository _walletRepository;
        public CreditRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper, IWalletRepository walletRepository) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
            _walletRepository = walletRepository;
        }



        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var credit = await _NarijeDBContext.Credits
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<CreditResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();


            return new ApiOkResponse(_Message: "SUCCEED", _Data: credit);
        }
        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Credit");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);
            try
            {
                var Q = _NarijeDBContext.Credits
                            .ProjectTo<CreditResponse>(_IMapper.ConfigurationProvider);

                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                var Credits = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

                return new ApiOkResponse(_Message: "SUCCESS", _Data: Credits.Data, _Meta: Credits.Meta, _Header: header);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: "error get data");
            }


        }
        #endregion


        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(CreditInsertRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.allDate))
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "لطفا دیتا را وارد کنید");

            List<CreditInsertArray> creditRequests;

            try
            {
                creditRequests = JsonConvert.DeserializeObject<List<CreditInsertArray>>(request.allDate);
            }
            catch (JsonException ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }

            try
            {
                if (request.customerId == 0)
                {
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "customerId خطایه سیستمی");
                }

                foreach (var item in creditRequests)
                {


                    if (item.Month == null || item.Year == null || request.customerId == 0)
                        return new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: "Month, Year, and CustomerId cannot be null.");

                    Credit credit;
                    if (item.Id.HasValue)
                    {
                        credit = await _NarijeDBContext.Credits.FindAsync(item.Id);
                        if (credit == null)
                        {
                            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: $"{item.Id} خطایه سیستمی");
                        }


                        // credit.CustomerId = request.customerId;
                        // credit.Id = (int)item.Id;
                        credit.Value = (long)item.Value;
                        // credit.DateTime = item.DateTime;
                        //  credit.Year = item.Year;
                        // credit.Month = item.Month;

                        _NarijeDBContext.Credits.Update(credit);
                    }
                    else
                    {
                        credit = new Credit
                        {
                            CustomerId = request.customerId,
                            DateTime = item.DateTime,
                            Value = (long)item.Value,
                            Riched = false,
                            Year = item.Year,
                            Month = item.Month
                        };

                        await _NarijeDBContext.Credits.AddAsync(credit);
                    }
                }

                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

                var newCredit = await _NarijeDBContext.Credits
              .Where(c => c.CustomerId == request.customerId)
              .ToListAsync();
                var newCreditDto = newCredit.Select(c => new
                {
                    id = c.Id,
                    customerId = c.CustomerId,
                    value = c.Value,
                    dateTime = c.DateTime,
                    year = c.Year,
                    month = c.Month,
                    riched = c.Riched
                }).ToList();
                return new ApiOkResponse(_Message: "SUCCEED", _Data: newCreditDto);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }


        }
        #endregion%

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(CreditEditRequest request)
        {
            var Credit = await _NarijeDBContext.Credits
                                                 .Where(A => A.Id == request.id)
                                                 .FirstOrDefaultAsync();
            if (Credit is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Credit.Value = request.value;
            Credit.CustomerId = request.customerId;
            Credit.DateTime = request.dateTime;

            _NarijeDBContext.Credits.Update(Credit);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Credit.Id);

        }
        #endregion


        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Credit = await _NarijeDBContext.Credits
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Credit is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            try
            {
                _NarijeDBContext.Credits.Remove(Credit);

                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

                return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

            }
            catch (Exception ex)
            {
                if ((ex.InnerException != null) && (((Microsoft.Data.SqlClient.SqlException)ex.InnerException).Number == 547))
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات در حال استفاده قابل حذف نیست");
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات حذف نشد! دوباره سعی کنید");
            }

        }
        #endregion

    }
}


