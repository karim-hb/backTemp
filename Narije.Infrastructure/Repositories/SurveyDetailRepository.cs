using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.SurveyDetail;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class SurveyDetailRepository : BaseRepository<SurveyDetail>, ISurveyDetailRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public SurveyDetailRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        { 
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var SurveyDetail = await _NarijeDBContext.SurveyDetails
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<SurveyDetailResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: SurveyDetail);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "SurveyDetail");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.SurveyDetails
                        .ProjectTo<SurveyDetailResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var SurveyDetails = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: SurveyDetails.Data, _Meta: SurveyDetails.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(SurveyDetailInsertRequest request)
        {
            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "مستقیم قابل اجرا نیست");

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(SurveyDetailEditRequest request)
        {
            var SurveyDetail = await _NarijeDBContext.SurveyDetails
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (SurveyDetail is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            SurveyDetail.SurveyId = request.surveyId;
            SurveyDetail.SurveyItemId = request.surveyItemId;
            if(request.surveyValueId != null)
                SurveyDetail.SurveyValueId = request.surveyValueId;
            SurveyDetail.Value = request.value;



            _NarijeDBContext.SurveyDetails.Update(SurveyDetail);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(SurveyDetail.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var SurveyDetail = await _NarijeDBContext.SurveyDetails
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (SurveyDetail is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.SurveyDetails.Remove(SurveyDetail);

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
        public async Task<ApiResponse> CloneAsync(SurveyDetailCloneRequest request)
        {
            var SurveyDetail = await _NarijeDBContext.SurveyDetails
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (SurveyDetail is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


