using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.AccessProfile;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class AccessProfileRepository : BaseRepository<AccessProfile>, IAccessProfileRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public AccessProfileRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var AccessProfile = await _NarijeDBContext.AccessProfiles
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<AccessProfileResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: AccessProfile);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "AccessProfile");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.AccessProfiles
                .Select(ap => new AccessProfileResponse
                {
                    id = ap.Id,
                    title = ap.Title,
                    users = string.Join(", ", ap.Users.Select(u => u.Fname + " " + u.Lname)) 
                });

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var AccessProfiles = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: AccessProfiles.Data, _Meta: AccessProfiles.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(AccessProfileInsertRequest request)
        {
            var AccessProfile = new AccessProfile()
            {
                Title = request.title,

            };


            await _NarijeDBContext.AccessProfiles.AddAsync(AccessProfile);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(AccessProfile.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(AccessProfileEditRequest request)
        {
            var AccessProfile = await _NarijeDBContext.AccessProfiles
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (AccessProfile is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            AccessProfile.Title = request.title;



            _NarijeDBContext.AccessProfiles.Update(AccessProfile);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(AccessProfile.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var AccessProfile = await _NarijeDBContext.AccessProfiles
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (AccessProfile is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.AccessProfiles.Remove(AccessProfile);

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
        public async Task<ApiResponse> CloneAsync(AccessProfileCloneRequest request)
        {
            var AccessProfile = await _NarijeDBContext.AccessProfiles
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (AccessProfile is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


