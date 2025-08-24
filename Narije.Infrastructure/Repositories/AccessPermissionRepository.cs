using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.AccessPermission;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class AccessPermissionRepository : BaseRepository<AccessPermission>, IAccessPermissionRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public AccessPermissionRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var AccessPermission = await _NarijeDBContext.AccessPermissions
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<AccessPermissionResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: AccessPermission);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "AccessPermission");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.AccessPermissions
                        .ProjectTo<AccessPermissionResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var AccessPermissions = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: AccessPermissions.Data, _Meta: AccessPermissions.Meta, _Header: header);

        }
        #endregion

        #region GetAllByAccessIdAsync
        // ------------------
        //  GetAllByAccessIdAsync
        // ------------------
        public async Task<ApiResponse> GetAllByAccessIdAsync(int? page, int? limit, int accessId)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "AccessPermission");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.AccessPermissions
                        .Where(A => A.AccessId == accessId)
                        .ProjectTo<AccessPermissionResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var AccessPermissions = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: AccessPermissions.Data, _Meta: AccessPermissions.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(AccessPermissionInsertRequest request)
        {
            var profile = await _NarijeDBContext.AccessProfiles.Where(A => A.Id == request.accessId)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
            if (profile is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات پروفایل یافت نشد");

            var permission = await _NarijeDBContext.Premissions.Where(A => A.Id == request.permissionId)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
            if (permission is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات دسترسی یافت نشد");


            var exists = await _NarijeDBContext.AccessPermissions.Where(A => A.PermissionId == request.permissionId && A.AccessId == request.accessId)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
            if(exists != null)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات تکراری می باشد");

            var AccessPermission = new AccessPermission()
            {
                AccessId = request.accessId,
                PermissionId = request.permissionId,

            };

            await _NarijeDBContext.AccessPermissions.AddAsync(AccessPermission);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(AccessPermission.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(AccessPermissionEditRequest request)
        {
            return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "متد وچود ندارد");
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int accessId, int permissionId)
        {
            var AccessPermission = await _NarijeDBContext.AccessPermissions
                                              .Where(A => A.AccessId == accessId && A.PermissionId == permissionId)
                                              .FirstOrDefaultAsync();
            if (AccessPermission is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.AccessPermissions.Remove(AccessPermission);

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
        public async Task<ApiResponse> CloneAsync(AccessPermissionCloneRequest request)
        {
            var AccessPermission = await _NarijeDBContext.AccessPermissions
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (AccessPermission is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


