using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Food;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System.Globalization;
using System.IO;
using Narije.Core.DTOs.Enum;
using System.Text.Json;
using Narije.Core.DTOs.ViewModels.MenuLog;

namespace Narije.Infrastructure.Repositories
{
    public class MenuLogRepository : BaseRepository<MenuLog>, IMenuLogRepository
    {
        public MenuLogRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
         base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }
        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit, int menuInfo)
        {
            try
            {
                if ((page is null) || (page == 0))
                    page = 1;
                if ((limit is null) || (limit == 0))
                    limit = 30;

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "MenuLog");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.MenuLogs
                    .Where(c => c.MenuInfoId == menuInfo)
                    .ProjectTo<MenuLogResponse>(_IMapper.ConfigurationProvider);

                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                var MenuLog = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

                return new ApiOkResponse(
                    _Message: "SUCCESS",
                    _Data: MenuLog?.Data ?? Enumerable.Empty<MenuLogResponse>(),
                    _Meta: MenuLog.Meta,
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

        #endregion
    }
}
