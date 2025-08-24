using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Permission;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public PermissionRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Permission = await _NarijeDBContext.Premissions
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<PermissionResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Permission);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Permission");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            //var Q = _NarijeDBContext.Premissions
            //s            .ProjectTo<PermissionResponse>(_IMapper.ConfigurationProvider);
            var Q = _NarijeDBContext.Premissions
                                    .Where(A => A.ParentId == null)
                                    .OrderBy(A => A.Priority)
                                    .Select(A => new
                                    {
                                        id = A.Id,
                                        title = A.Title,
                                        active = A.Active,
                                        chileds = A.Childrens.OrderBy(B => B.Priority).Select(B => new
                                        {
                                            title = B.Title,
                                            active = B.Active,
                                            id = B.Id,
                                            childs = B.Childrens.OrderBy(C => C.Priority).Select(C => new
                                            {
                                                id = C.Id,
                                                title = C.Title,
                                                module = C.Module,
                                                value = C.Value,
                                                active = C.Active,
                                            }).ToList()
                                        }).ToList()
                                    })
                                    .AsNoTracking();

            Q = Q.QueryDynamic(query.Search, query.Filter);//.OrderDynamic(query.Sort);

            var Permissions = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Permissions.Data, _Meta: Permissions.Meta, _Header: header);

        }
        #endregion

    }
}


