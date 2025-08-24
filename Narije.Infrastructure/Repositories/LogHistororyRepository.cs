using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Setting;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.LogHistroy;
using System.Text.Json;

namespace Narije.Infrastructure.Repositories
{
    public class LogHistororyRepository : BaseRepository<LogHistory>, ILogHistoryRepository
    {
        // ------------------
        // Constructor
        // ------------------
        public LogHistororyRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var Setting = await _NarijeDBContext.LogHistory
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<LogHistoryResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: Setting);
        }
        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit, string entityName, int id)
        {
            if ((page is null) || (page == 0))
                page = 1;
            if ((limit is null) || (limit == 0))
                limit = 30;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "LogHistory");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.LogHistory.Where(l => l.EntityName == entityName && l.EntityId == id)
                        .ProjectTo<LogHistoryResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Settings = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            var entityHeaders = await _NarijeDBContext.Headers
                .Where(h => h.TableName.ToLower() == entityName.ToLower())
                .Select(h => new {
                    Name = h.FieldName.ToLower(),
                    h.Title
                })
                .ToListAsync();

            foreach (var item in Settings.Data)
            {
                if (!string.IsNullOrEmpty(item.changed))
                {
                    var changedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(item.changed);
                    var processedDict = new Dictionary<string, object>();

                    foreach (var key in changedDict.Keys)
                    {
                        bool isBefore = key.StartsWith("before_");
                        string fieldName = isBefore ? key.Substring(7) : key;

                        var headerField = entityHeaders.FirstOrDefault(h => h.Name == fieldName.ToLower());
                        if (headerField == null) continue;

                        string title = headerField.Title;
                        if (isBefore)
                            title += " (قبل از تغییر)";

                        processedDict[title] = changedDict[key];
                    }

                    item.changed = JsonSerializer.Serialize(processedDict);
                }
            }

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Settings.Data, _Meta: Settings.Meta, _Header: header);
        }
        #endregion


    }
}
