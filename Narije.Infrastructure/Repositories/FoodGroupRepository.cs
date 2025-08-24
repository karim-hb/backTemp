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
using Narije.Core.DTOs.ViewModels.Export;
using Narije.Core.DTOs.ViewModels.VCustomer;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Web;

namespace Narije.Infrastructure.Repositories
{
    public class FoodGroupRepository : BaseRepository<FoodGroup>, IFoodGroupRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public FoodGroupRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var FoodGroup = await _NarijeDBContext.FoodGroups
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<FoodGroupResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: FoodGroup);
        }
        #endregion
        #region Export
        public new async Task<ApiResponse> ExportAsync()
        {
            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            List<FieldResponse> headers = new();


            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "FoodGroup", true);

            var query = FilterHelper.GetQuery(dbheader, _IHttpContextAccessor);


            result.header = dbheader.Select(A => A.title).ToList();


            var Q = _NarijeDBContext.FoodGroups
                        .ProjectTo<FoodGroupResponse>(_IMapper.ConfigurationProvider);

            var Param = HttpUtility.ParseQueryString(_IHttpContextAccessor.HttpContext.Request.QueryString.ToString());
            var key = Param.AllKeys.Where(A => A.Equals("ids")).FirstOrDefault();
            if (key != null)
            {
                string ids = "";
                ids = Param[key.ToString()];
                if (ids != "")
                {
                    int[] nids = ids.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    Q = Q.Where("(@0.Contains(id))", nids);
                }
            }

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "دسترسی ندارید");




            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);



            var data = await Q.ToListAsync<object>();

            var MapToTable = true;


            result.body = ExportHelper.MakeResult(data, dbheader, MapToTable);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "FoodGroup");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.FoodGroups
                        .ProjectTo<FoodGroupResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var FoodGroups = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: FoodGroups.Data, _Meta: FoodGroups.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(FoodGroupInsertRequest request)
        {
     
            var existingFoodGroups = await _NarijeDBContext.FoodGroups
                                            .Where(f => f.ArpaNumber == request.arpaNumber)
                                            .FirstOrDefaultAsync();

            if (existingFoodGroups != null)
            {
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status400BadRequest,
                    _Message: $"گروه کالا دیگری با کد کالا {request.arpaNumber} موجود است"
                );
            }

            var FoodGroup = new FoodGroup()
            {
                Title = request.title,
                InvoiceAddOn = request.invoiceAddOn ?? false,
                ArpaNumber = request.arpaNumber,
                Description = request.description,  
            };
            FoodGroup.GalleryId = await GalleryHelper.AddFromGallery(_NarijeDBContext, request.fromGallery);
            if (request.files != null)
            {
                var k = await GalleryHelper.AddToGallery(_NarijeDBContext, "FoodGroup", request.files.FirstOrDefault());
                if (k > 0)
                    FoodGroup.GalleryId = k;
            }


            await _NarijeDBContext.FoodGroups.AddAsync(FoodGroup);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(FoodGroup.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(FoodGroupEditRequest request)
        {

            var FoodGroup = await _NarijeDBContext.FoodGroups
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();


            if (FoodGroup is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");


         
            var existingFoodGroups = await _NarijeDBContext.FoodGroups
                                   .Where(f => f.ArpaNumber == request.arpaNumber)
                                   .FirstOrDefaultAsync();

            if (existingFoodGroups != null && existingFoodGroups.Id != request.id)
            {
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status400BadRequest,
                    _Message: $"گروه کالا  دیگری با کد کالا {request.arpaNumber} موجود است"
                );
            }
            FoodGroup.Title = request.title;
            FoodGroup.InvoiceAddOn = request.invoiceAddOn ?? false;
            FoodGroup.ArpaNumber = request.arpaNumber;
            FoodGroup.Description = request.description;
            FoodGroup.GalleryId = await GalleryHelper.EditFromGallery(_NarijeDBContext, FoodGroup.GalleryId, request.fromGallery);
            if (request.files != null)
                FoodGroup.GalleryId = await GalleryHelper.EditGallery(_NarijeDBContext, FoodGroup.GalleryId, "FoodGroup", request.files.FirstOrDefault());

            _NarijeDBContext.FoodGroups.Update(FoodGroup);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(FoodGroup.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var FoodGroup = await _NarijeDBContext.FoodGroups
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (FoodGroup is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.FoodGroups.Remove(FoodGroup);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);

        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(FoodGroupCloneRequest request)
        {
            var FoodGroup = await _NarijeDBContext.FoodGroups
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (FoodGroup is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


