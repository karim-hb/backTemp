using AutoMapper;
using AutoMapper.QueryableExtensions;
using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Narije.Core.DTOs.Public;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Narije.Core.DTOs.ViewModels.Export;
using Narije.Core.DTOs.ViewModels.FoodPrice;
using Narije.Core.DTOs.ViewModels.WalletPayment;
using Narije.Core.DTOs.ViewModels.Food;
using Narije.Core.DTOs.Enum;
using System.Security.Claims;
using Narije.Core.DTOs.ViewModels.User;

namespace Narije.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly IConfiguration _IConfiguration;
        protected readonly IHttpContextAccessor _IHttpContextAccessor;
        protected readonly NarijeDBContext _NarijeDBContext;
        protected readonly IMapper _IMapper;

        // ------------------
        // Constructor
        // ------------------
        public BaseRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper)
        {
            this._IConfiguration = _IConfiguration;
            this._IHttpContextAccessor = _IHttpContextAccessor;
            this._NarijeDBContext = _NarijeDBContext;
            this._IMapper = _IMapper;
        }

        public async Task<ApiResponse> ExportAsync()
        {
            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            List<FieldResponse> headers = new();

            var TableName = _NarijeDBContext.Set<T>().EntityType.ClrType.Name;

            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, TableName, true);

            result.header = dbheader.Select(A => A.title).ToList();

            var query = FilterHelper.GetQuery(dbheader, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Set<T>().AsQueryable();

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

            var user = await _NarijeDBContext.Users.Where(A => A.Id == Int32.Parse(Identity.FindFirst("Id").Value)).AsNoTracking().FirstOrDefaultAsync();

            if (user is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status403Forbidden, _Message: "کاربر یافت نشد");

            switch (user.Role)
            {
                case (int)EnumRole.user:
                    Q = Q.Where("id = @0", user.Id);
                    break;
                case (int)EnumRole.customer:
                    Q = Q.Where("CustomerId = @0", user.CustomerId);
                    break;
            }

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            List<object> data = new();
            switch(TableName.ToLower())
            {
                case "user":
                    data = await Q.ProjectTo<UserResponse>(_IMapper.ConfigurationProvider).ToListAsync<object>();
                    break;
                case "food":
                    data = await Q.ProjectTo<FoodResponse>(_IMapper.ConfigurationProvider).ToListAsync<object>();
                    break;
                case "walletpayment":
                    data = await Q.ProjectTo<WalletPaymentResponse>(_IMapper.ConfigurationProvider).ToListAsync<object>();
                    break;
                default:
                    data = await Q.ToListAsync<object>();
                    break;
            }
            var MapToTable = true;
            if(TableName == "Supplier")
                MapToTable= false;

            result.body = ExportHelper.MakeResult(data, dbheader, MapToTable);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);
        }
    }
}
