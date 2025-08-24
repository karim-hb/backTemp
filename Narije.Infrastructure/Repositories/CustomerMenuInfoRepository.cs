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
using Narije.Core.DTOs.ViewModels.CustomerMenuInfo;
namespace Narije.Infrastructure.Repositories
{
    public  class CustomerMenuInfoRepository : BaseRepository<CustomerMenuInfo>, ICustomerMenuInfo
    {
        public CustomerMenuInfoRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
        base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        public async Task<ApiResponse> GetAllAsync(int customerId)
        {

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "CustomerMenuInfo");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);


            var Q = _NarijeDBContext.CustomerMenuInfo.Where(c => c.CustomerId == customerId)
                        .ProjectTo<CustomerMenuInfoResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var FoodGroups = await Q.GetPaged(Page: 1, Limit: 1000);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: FoodGroups.Data, _Meta: FoodGroups.Meta, _Header: header);

        }
        public static (int Year, int Month) GetCurrentShamsiDate()
        {
            var persianCalendar = new System.Globalization.PersianCalendar();
            var now = DateTime.Now;
            int year = persianCalendar.GetYear(now);
            int month = persianCalendar.GetMonth(now);
            return (year, month);
        }
        public async Task<ApiResponse> EditInsertAsync(CustomerMenuInfoRequest[] requests)
        {
            using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();
            try
            {
                var (currentShamsiYear, currentShamsiMonth) = GetCurrentShamsiDate();

                foreach (var request in requests)
                {
                    if (request.year < currentShamsiYear || (request.year == currentShamsiYear && request.month < currentShamsiMonth))
                    {
                        continue; 
                    }

                    var preMenuId = await _NarijeDBContext.CustomerMenuInfo
                        .Where(c => c.CustomerId == request.customerId && c.Month == request.month && c.Year == request.year)
                        .FirstOrDefaultAsync();

                    if (preMenuId != null && preMenuId.MenuInfoId != request.menuInfoId)
                    {
                        bool hasReserves = await _NarijeDBContext.Reserves
                            .AnyAsync(r => r.CustomerId == request.customerId && r.MenuInfo == request.menuInfoId && r.Num > 0);

                        var childCompaniesWithActiveReserves = await _NarijeDBContext.Reserves
                            .Where(r => r.Num > 0 && r.MenuInfo == request.menuInfoId)
                            .Join(
                                _NarijeDBContext.Customers,
                                reserve => reserve.CustomerId,
                                customer => customer.Id,
                                (reserve, customer) => new { customer.ParentId, customer.Title, customer.Id }
                            )
                            .Where(c => c.ParentId == request.customerId)
                            .Select(c => c.Title)
                            .ToListAsync();

                        if (hasReserves || childCompaniesWithActiveReserves.Any())
                        {
                            var companiesMessage = childCompaniesWithActiveReserves.Any()
                                ? $"و شرکت‌های زیرمجموعه فعال: {string.Join(", ", childCompaniesWithActiveReserves)}"
                                : string.Empty;

                            await transaction.RollbackAsync();
                            return new ApiErrorResponse(
                                _Code: StatusCodes.Status400BadRequest,
                                _Message: $"این منو به دلیل داشتن رزرو فعال قابل ویرایش نیست (منو ماه {request.month}). {companiesMessage}"
                            );
                        }
                    }

                    var existingRecord = await _NarijeDBContext.CustomerMenuInfo
                        .FirstOrDefaultAsync(cmi => cmi.CustomerId == request.customerId &&
                                                    cmi.Month == request.month &&
                                                    cmi.Year == request.year);

                    if (existingRecord != null)
                    {
                        existingRecord.MenuInfoId = request.menuInfoId;
                        _NarijeDBContext.CustomerMenuInfo.Update(existingRecord);
                    }
                    else
                    {
                        var newRecord = new CustomerMenuInfo
                        {
                            CustomerId = request.customerId,
                            MenuInfoId = request.menuInfoId,
                            Month = request.month,
                            Year = request.year
                        };

                        await _NarijeDBContext.CustomerMenuInfo.AddAsync(newRecord);
                    }
                }

                var result = await _NarijeDBContext.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();
                    return new ApiOkResponse(_Message: "SUCCESS", _Data: null);
                }

                await transaction.RollbackAsync();
                return new ApiErrorResponse(StatusCodes.Status400BadRequest, "Failed to save changes.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiErrorResponse(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

    }
}
