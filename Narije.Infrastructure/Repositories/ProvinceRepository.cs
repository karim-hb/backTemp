using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Province;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class ProvinceRepository : BaseRepository<Province>, IProvinceRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public ProvinceRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        { 
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Province = await _NarijeDBContext.Provinces
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<ProvinceResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Province);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Province");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Provinces
                        .ProjectTo<ProvinceResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Provinces = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Provinces.Data, _Meta: Provinces.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(ProvinceInsertRequest request)
        {
            var Province = new Province()
            {
                Title = request.title,                Code = request.code,
            };


            await _NarijeDBContext.Provinces.AddAsync(Province);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Province.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(ProvinceEditRequest request)
        {
            var Province = await _NarijeDBContext.Provinces
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (Province is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Province.Title = request.title;            Province.Code = request.code;


            _NarijeDBContext.Provinces.Update(Province);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Province.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var Province = await _NarijeDBContext.Provinces
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Province is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.Provinces.Remove(Province);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(ProvinceCloneRequest request)
        {
            var Province = await _NarijeDBContext.Provinces
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Province is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


