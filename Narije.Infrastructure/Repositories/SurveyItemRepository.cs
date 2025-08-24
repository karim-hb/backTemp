using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.SurveyItem;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class SurveyItemRepository : BaseRepository<SurveyItem>, ISurveyItemRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public SurveyItemRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        { 
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var SurveyItem = await _NarijeDBContext.SurveyItems
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<SurveyItemResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: SurveyItem);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "SurveyItem");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.SurveyItems
                        .ProjectTo<SurveyItemResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var SurveyItems = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: SurveyItems.Data, _Meta: SurveyItems.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(SurveyItemInsertRequest request)
        {
            var SurveyItem = new SurveyItem()
            {
                Title = request.title,
                ItemType = request.itemType,
                Active = request.active,
                Value = request.value,
                HasSeparateItems = request.hasSeparateItems??false

            };


            await _NarijeDBContext.SurveyItems.AddAsync(SurveyItem);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(SurveyItem.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(SurveyItemEditRequest request)
        {
            var SurveyItem = await _NarijeDBContext.SurveyItems
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (SurveyItem is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            SurveyItem.Title = request.title;
            SurveyItem.ItemType = request.itemType;
            SurveyItem.Active = request.active;
            SurveyItem.Value = request.value;
            SurveyItem.HasSeparateItems = request.hasSeparateItems??false;

            _NarijeDBContext.SurveyItems.Update(SurveyItem);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(SurveyItem.Id);
        }
        #endregion

        #region EditActiveAsync
        // ------------------
        //  EditActiveAsync
        // ------------------
        public async Task<ApiResponse> EditActiveAsync(int id)
        {
            var Data = await _NarijeDBContext.SurveyItems
                                                  .Where(A => A.Id == id)
                                                  .FirstOrDefaultAsync();
            if (Data is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            Data.Active = !Data.Active;

            _NarijeDBContext.SurveyItems.Update(Data);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Data.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var SurveyItem = await _NarijeDBContext.SurveyItems
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (SurveyItem is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");


            try
            {
                _NarijeDBContext.SurveyItems.Remove(SurveyItem);

                var Result = await _NarijeDBContext.SaveChangesAsync();
                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

                return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

            }
            catch (Exception ex)
            {

            }

            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "آیتم نظر داده شده قابل حذف نیست");


        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(SurveyItemCloneRequest request)
        {
            var SurveyItem = await _NarijeDBContext.SurveyItems
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (SurveyItem is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


