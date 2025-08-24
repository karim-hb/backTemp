using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Customer;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Narije.Core.DTOs.ViewModels.VCustomer;
using Castle.DynamicProxy.Generators;
using Narije.Core.DTOs.Enum;
using System.Text.Json;
using Castle.Core.Resource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using System.Globalization;
using System.Drawing;
using System.Xml.Linq;
using System.IO;
using Narije.Core.DTOs.ViewModels.Export;
using Narije.Core.DTOs.ViewModels.Food;
using Narije.Core.DTOs.ViewModels.User;
using Narije.Core.DTOs.ViewModels.WalletPayment;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Web;
namespace Narije.Infrastructure.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {

        // ------------------
        // Constructor
        // ------------------
        private readonly LogHistoryHelper _logHistoryHelper;

        public CustomerRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper, LogHistoryHelper logHistoryHelper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
            _logHistoryHelper = logHistoryHelper;

        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            try
            {
                var Customer = await _NarijeDBContext.VCustomers
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<vCustomerSingleResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

                if (Customer == null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "Customer not found");

                var CustomerMeals = await _NarijeDBContext.CompanyMeal
                                                 .Where(m => m.CustomerId == id)
                                              .AsNoTracking()
                                                 .Select(m => new CustomerMeal
                                                 {
                                                     mealId = m.MealId,
                                                     maxReserveTime = m.MaxReserveTime,
                                                     maxNumberCanReserve = m.MaxNumberCanReserve,
                                                     deliverHour = m.DeliverHour,
                                                     active = m.Active,
                                                     foodServerNumber = m.FoodServerNumber,
                                                 })
                                                 .ToListAsync();
                var customerAccessories = await _NarijeDBContext.AccessoryCompany
                    .Where(a => a.CompanyId == id)
                    .AsNoTracking()
                    .Select(a => new
                    {
                        id = a.Id,
                        accessoryId = a.AccessoryId,
                        companyId = a.CompanyId,
                        numbers = a.Numbers,
                        price = a.Price
                    })
                    .ToListAsync();

                return new ApiOkResponse(_Message: "SUCCEED", _Data: new { Customer, CustomerMeals, customerAccessories });

            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }
        #endregion

        #region GetAllAsync
        // ------------------
        //  GetAllAsync
        // ------------------
        public async Task<ApiResponse> GetAllAsync(int? page, int? limit, bool? onlyBranch)

        {
            try
            {
                if ((page is null) || (page == 0))
                    page = 1;
                if ((limit is null) || (limit == 0))
                    limit = 30;

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Customer");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.VCustomers
                            .ProjectTo<VCustomerResponse>(_IMapper.ConfigurationProvider);

                if (onlyBranch == true)
                {
                    Q = Q.Where(c => c.parentId != null);
                }
                else
                {
                    Q = Q.Where(a => a.parentId == null);

                }

                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);


                var Customers = await Q.GetPaged(Page: page.Value, Limit: limit.Value);



                return new ApiOkResponse(_Message: "SUCCESS", _Data: Customers.Data, _Meta: Customers.Meta, _Header: header);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }


        }
        #endregion

        #region GetAllCustomerReport
        // ------------------
        //  GetAllCustomerReport
        // ------------------
        public async Task<ApiResponse> GetAllCustomerReport(int? page, int? limit)

        {
            try
            {
                if ((page is null) || (page == 0))
                    page = 1;
                if ((limit is null) || (limit == 0))
                    limit = 30;

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "CustomerReport");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.VCustomers
                            .ProjectTo<VCustomerReportResponse>(_IMapper.ConfigurationProvider);

                Q = Q.Where(c => c.parentId != null);
                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);


                var Customers = await Q.GetPaged(Page: page.Value, Limit: limit.Value);
                var ids = Customers.Data.Select(A => A.parentId);
                var wallparentsets = await _NarijeDBContext.Customers.Where(A => ids.Contains(A.ParentId)).AsNoTracking().ToListAsync();


                foreach (var customer in Customers.Data)
                {
                    customer.parentCode = wallparentsets.FirstOrDefault(A => A.Id == customer.id).Code;
                    customer.customerId = customer.id;
                }



                return new ApiOkResponse(_Message: "SUCCESS", _Data: Customers.Data, _Meta: Customers.Meta, _Header: header);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }


        }
        #endregion

        #region GetAllBranchesAsync
        // ------------------
        //  GetAllBranchesAsync
        // ------------------
        public async Task<ApiResponse> GetAllBranchesAsync(int? page, int? limit, int companyId)

        {
            try
            {
                if ((page is null) || (page == 0))
                    page = 1;
                if ((limit is null) || (limit == 0))
                    limit = 30;

                var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "CustomerBranches");
                var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

                var Q = _NarijeDBContext.VCustomers
                            .ProjectTo<VCustomerResponse>(_IMapper.ConfigurationProvider);
                var parent = await _NarijeDBContext.VCustomers.Where(c => c.Id == companyId).FirstOrDefaultAsync();
                Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

                Q = Q.Where(a => a.parentId == companyId);

                var Customers = await Q.GetPaged(Page: page.Value, Limit: limit.Value);




                return new ApiOkResponse(_Message: "SUCCESS", _Data: Customers.Data, _Meta: Customers.Meta, _Header: header, _ExtraObject: new { priceType = parent.PriceType, avragePrice = parent.AvragePrice });
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }


        }
        #endregion
        #region GetAllBranchesAsync
        // ------------------
        //  GetAllBranchesAsync
        // ------------------
        public async Task<ApiResponse> GetAllCustomerMenuAsync(int customerId)

        {
            try
            {




                var menuInfo = await _NarijeDBContext.CustomerMenuInfo.Where(c => c.CustomerId == customerId).Select(c => new
                {


                    title = c.MenuInfo.Title,
                    month = c.MenuInfo.Month,
                    year = c.MenuInfo.Year,
                    id = c.MenuInfoId,


                })
                .ToListAsync();







                return new ApiOkResponse(_Message: "SUCCESS", _Data: menuInfo);
            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }


        }
        #endregion

        #region ExportBranch
        public async Task<ApiResponse> ExportBranch(int companyId)
        {
            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            List<FieldResponse> headers = new();


            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "CustomerBranches", true);

            var query = FilterHelper.GetQuery(dbheader, _IHttpContextAccessor);


            result.header = dbheader.Select(A => A.title).ToList();


            var Q = _NarijeDBContext.VCustomers
                        .ProjectTo<VCustomerResponse>(_IMapper.ConfigurationProvider);

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


            Q = Q.Where(a => a.parentId == companyId);


            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);



            var data = await Q.ToListAsync<object>();

            var MapToTable = true;


            result.body = ExportHelper.MakeResult(data, dbheader, MapToTable);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);
        }

        #endregion
        #region ExportAsync
        public async Task<ApiResponse> ExportAsync(bool justBranches)
        {
            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            List<FieldResponse> headers = new();


            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, justBranches ? "CustomerBranches": "Customer", true);

            var query = FilterHelper.GetQuery(dbheader, _IHttpContextAccessor);


            result.header = dbheader.Select(A => A.title).ToList();


            var Q = _NarijeDBContext.VCustomers
                        .ProjectTo<VCustomerResponse>(_IMapper.ConfigurationProvider);

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

            if (justBranches)
            {
                Q = Q.Where(a => a.parentId != null);
            }
            else
            {
                Q = Q.Where(a => a.parentId == null);
            }
          


            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);



            var data = await Q.ToListAsync<object>();

            var MapToTable = true;


            result.body = ExportHelper.MakeResult(data, dbheader, MapToTable);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);
        }

        #endregion
        #region GetLastCodeAsync
        // ------------------
        //  GetLastCodeAsync
        // ------------------
        public async Task<ApiResponse> GetLastCodeAsync(int? companyId)
        {
            var query = _NarijeDBContext.VCustomers.AsQueryable();

            if (companyId == null)
            {
                query = query.Where(x => x.ParentId == null);
            }
            else
            {
                query = query.Where(x => x.ParentId == companyId);
            }

            var lastCode = await query
                .OrderByDescending(x => x.Code)
                .Select(x => x.Code)
                .FirstOrDefaultAsync();

            if (lastCode == null)
            {
                if (companyId == null)
                {
                    lastCode = "300000";
                }
                else
                {
                    var lc = await _NarijeDBContext.VCustomers.Where(x => x.Id == companyId).Select(x => x.Code)
                        .FirstOrDefaultAsync();

                    lastCode = lc + "00";
                }
            }

            return new ApiOkResponse(_Message: "SUCCEED", _Data: lastCode);
        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(CustomerInsertRequest request)
        {
            try
            {
                var Customer = new Customer();

                if (!string.IsNullOrEmpty(request.title)) Customer.Title = request.title;
                if (request.cancelTime.HasValue) Customer.CancelTime = request.cancelTime;
                if (request.guestTime.HasValue) Customer.GuestTime = request.guestTime;
                if (request.reserveTime != null) Customer.ReserveTime = request.reserveTime;

                if (request.reserveAfter > 0) Customer.ReserveAfter = request.reserveAfter;
                if (request.reserveTo.HasValue) Customer.ReserveTo = request.reserveTo;
                if (request.cancelPercent > 0) Customer.CancelPercent = request.cancelPercent;
                if (request.cancelPercentPeriod > 0) Customer.CancelPercentPeriod = request.cancelPercentPeriod;

                if (request.parentId.HasValue)
                {
                    Customer.ParentId = request.parentId;
                    var Parent = _NarijeDBContext.Customers.Where(c => c.Id == request.parentId).FirstOrDefault();
                    if (Parent == null)
                    {
                        return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "شرکت مادر نا معتبر می باشد");
                    }
                    Customer.ContractStartDate = Parent.ContractStartDate;
                    Customer.ContractEndDate = Parent.ContractEndDate;
                    Customer.Active = request.active;
                }
                else
                {
                    if (request.contractStartDate > DateTime.MinValue) Customer.ContractStartDate = request.contractStartDate;
                    if (request.contractEndDate.HasValue)
                    {
                        Customer.ContractEndDate = request.contractEndDate;
                        Customer.Active = request.contractEndDate < DateTime.Now ? false : request.active;
                    }

                }



                if (!string.IsNullOrEmpty(request.address)) Customer.Address = request.address;
                if (!string.IsNullOrEmpty(request.tel)) Customer.Tel = request.tel;
                if (request.foodType > 0) Customer.FoodType = request.foodType;
                Customer.ShowPrice = request.showPrice;
                if (request.addCreditToPrevCredit.HasValue) Customer.AddCreditToPrevCredit = request.addCreditToPrevCredit;
                if (request.minReserve > 0) Customer.MinReserve = request.minReserve;
                if (!string.IsNullOrEmpty(request.economicCode)) Customer.EconomicCode = request.economicCode;
                if (!string.IsNullOrEmpty(request.nationalId)) Customer.NationalId = request.nationalId;
                if (!string.IsNullOrEmpty(request.regNumber)) Customer.RegNumber = request.regNumber;
                if (!string.IsNullOrEmpty(request.mobile)) Customer.Mobile = request.mobile;
                if (request.cityId.HasValue) Customer.CityId = request.cityId;
                if (request.provinceId.HasValue) Customer.ProvinceId = request.provinceId;
                if (!string.IsNullOrEmpty(request.postalCode)) Customer.PostalCode = request.postalCode;

                Customer.Subsidy = request.subsidy ?? 0;
                Customer.PayType = request.payType ?? 0;
                if (request.maxMealCanReserve.HasValue) Customer.MaxMealCanReserve = request.maxMealCanReserve;
                if (!string.IsNullOrEmpty(request.mealType)) Customer.MealType = request.mealType;
                if (request.jobId.HasValue) Customer.JobId = request.jobId;
                if (request.settlementId.HasValue) Customer.SettlementId = request.settlementId;
                if (request.dishId.HasValue) Customer.DishId = request.dishId;
                if (request.isLegal.HasValue) Customer.IsLegal = request.isLegal;
                if (!string.IsNullOrEmpty(request.code)) Customer.Code = request.code;
                if (!string.IsNullOrEmpty(request.lat)) Customer.Lat = request.lat;
                if (!string.IsNullOrEmpty(request.lng)) Customer.Lng = request.lng;
                if (!string.IsNullOrEmpty(request.agentFullName)) Customer.AgentFullName = request.agentFullName;
                if (request.companyType.HasValue) Customer.CompanyType = request.companyType;

                if (request.branchForSaturday.HasValue) Customer.BranchForSaturday = request.branchForSaturday;
                if (request.branchForSunday.HasValue) Customer.BranchForSunday = request.branchForSunday;
                if (request.branchForMonday.HasValue) Customer.BranchForMonday = request.branchForMonday;
                if (request.branchForTuesday.HasValue) Customer.BranchForTuesday = request.branchForTuesday;
                if (request.branchForWednesday.HasValue) Customer.BranchForWednesday = request.branchForWednesday;
                if (request.branchForThursday.HasValue) Customer.BranchForThursday = request.branchForThursday;
                if (request.branchForFriday.HasValue) Customer.BranchForFriday = request.branchForFriday;


                if (request.branchForFriday.HasValue) Customer.BranchForFriday = request.branchForFriday;
                if (!string.IsNullOrEmpty(request.deliverFullName)) Customer.DeliverFullName = request.deliverFullName;
                if (!string.IsNullOrEmpty(request.deliverPhoneNumber)) Customer.DeliverPhoneNumber = request.deliverPhoneNumber;
                if (request.shippingFee.HasValue) Customer.ShippingFee = request.shippingFee;
                if (request.avragePrice.HasValue) Customer.AvragePrice = request.avragePrice;
                if (request.priceType.HasValue) Customer.PriceType = request.priceType;

                await _NarijeDBContext.Customers.AddAsync(Customer);

                var Result = await _NarijeDBContext.SaveChangesAsync();

                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");


                if (request.CustomerMeal != null && request.CustomerMeal.Any())
                {
                    var customerMeals = request.CustomerMeal.Select(meal => new CompanyMeal()
                    {
                        CustomerId = Customer.Id,
                        MealId = meal.mealId,
                        MaxReserveTime = meal.maxReserveTime,
                        MaxNumberCanReserve = meal.maxNumberCanReserve,
                        DeliverHour = meal.deliverHour,
                        Active = meal.active,
                        FoodServerNumber = meal.foodServerNumber,
                    }).ToList();

                    await _NarijeDBContext.CompanyMeal.AddRangeAsync(customerMeals);
                }

                if (request.customerAccessory != null && request.customerAccessory.Any())
                {
                    var customerAccessories = request.customerAccessory.Select(accessory => new AccessoryCompany()
                    {
                        CompanyId = Customer.Id,
                        AccessoryId = accessory.accessoryId,
                        Numbers = accessory.numbers,
                        Price = accessory.price
                    }).ToList();

                    await _NarijeDBContext.AccessoryCompany.AddRangeAsync(customerAccessories);
                }


                Result = await _NarijeDBContext.SaveChangesAsync();
                if (Result < 0)
                    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");


                await _logHistoryHelper.AddLogHistoryAsync(
                      "Customer",
                         Customer.Id,
                         EnumLogHistroyAction.create,
                        EnumLogHistorySource.site,
                       JsonSerializer.Serialize(request),
                       true
                      );

                return await GetAsync(Customer.Id);

            }
            catch (Exception ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(CustomerEditRequest request)
        {
            await using var transaction = await _NarijeDBContext.Database.BeginTransactionAsync();
            try
            {
                var Customer = await _NarijeDBContext.Customers
                                                      .Where(A => A.Id == request.id)
                                                      .FirstOrDefaultAsync();
                if (Customer is null)
                    return new ApiErrorResponse(StatusCodes.Status404NotFound, "اطلاعات جهت ویرایش یافت نشد");



                var childCustomers = await _NarijeDBContext.Customers
                    .Where(c => c.ParentId == Customer.Id)
                    .ToListAsync();


                var childCustomerIds = childCustomers.Select(c => c.Id).ToList();
                childCustomerIds.Add(Customer.Id);

                var activeReserves = await _NarijeDBContext.Reserves
                    .Where(r => childCustomerIds.Contains(r.CustomerId) && r.DateTime > DateTime.Now)
                    .Include(r => r.Food)
                    .ToListAsync();

                if (request.priceType != null && request.priceType != Customer.PriceType && activeReserves.Any())
                {
                    return new ApiErrorResponse(StatusCodes.Status400BadRequest, "شعبات این شرکت دارای رزرو فعال هستند به همین دلیل نوع قیمت گذاری قابل ویرایش نیست");
                }

                if (request.avragePrice != null && request.avragePrice != Customer.AvragePrice)
                {
                    var avgPriceReserves = activeReserves.Where(r => r.PriceType == (int)EnumPrice.average || r.PriceType == (int)EnumPrice.justVipFromMenu).ToList();
                    foreach (var reserve in avgPriceReserves)
                    {
                        if (reserve.PriceType == (int)EnumPrice.justVipFromMenu && reserve.Food.Vip == true)
                        {
                            continue;
                        }
                        else
                        {
                            reserve.Price = request.avragePrice.Value;
                            if (reserve.PayType == (int)EnumInvoicePayType.debit && request.avragePrice.HasValue && Customer.AvragePrice.HasValue)
                            {
                                await UpdateReservesAndWallet(reserve, request.avragePrice.Value, Customer.AvragePrice.Value);
                            }
                        }

                    }
                    _NarijeDBContext.Reserves.UpdateRange(avgPriceReserves);
                }
                if (request.contractEndDate != Customer.ContractEndDate)
                {
                    foreach (var child in childCustomers)
                    {
                        child.ContractEndDate = request.contractEndDate;
                        _NarijeDBContext.Customers.Update(child);
                    }

                    var reservesToDelete = await _NarijeDBContext.Reserves
                        .Where(r => childCustomerIds.Contains(r.CustomerId)
                            && r.DateTime > request.contractEndDate)
                        .ToListAsync();

                    if (reservesToDelete.Any())
                    {
                        foreach (var reserve in reservesToDelete)
                        {
                            if (reserve.PayType == (int)EnumInvoicePayType.debit )
                            {
                                await UpdateReservesAndWallet(reserve, reserve.Price, 0);
                            }

                        }
                        _NarijeDBContext.Reserves.RemoveRange(reservesToDelete);
                    }
                }
                var changes = LogHistoryHelper.GetEntityChanges(request, Customer);
                Customer.Title = request.title;
                if (request.cancelTime != null)
                    Customer.CancelTime = request.cancelTime;
                if (request.guestTime != null)
                    Customer.GuestTime = request.guestTime;
                Customer.ReserveAfter = request.reserveAfter;
                if (request.reserveTo != null)
                    Customer.ReserveTo = request.reserveTo;
                Customer.CancelPercent = request.cancelPercent;
                Customer.CancelPercentPeriod = request.cancelPercentPeriod;
                Customer.ContractStartDate = request.contractStartDate;
                Customer.ContractEndDate = request.contractEndDate;
                Customer.AddCreditToPrevCredit = request.addCreditToPrevCredit;
                Customer.Active = request.contractEndDate < DateTime.Now ? false : request.active;
                Customer.Address = request.address;
                Customer.Tel = request.tel;
                Customer.FoodType = request.foodType;
                Customer.ReserveTime = request.reserveTime;
                Customer.ShowPrice = request.showPrice;
                Customer.MinReserve = request.minReserve;
                Customer.EconomicCode = request.economicCode;
                Customer.NationalId = request.nationalId;
                Customer.RegNumber = request.regNumber;
                Customer.Mobile = request.mobile;
                if (request.cityId != null)
                    Customer.CityId = request.cityId;
                if (request.provinceId != null)
                    Customer.ProvinceId = request.provinceId;
                Customer.PostalCode = request.postalCode;
                Customer.PayType = request.payType ?? 0;
                Customer.Subsidy = request.subsidy ?? 0;
                Customer.MealType = request.mealType;
                Customer.MaxMealCanReserve = request.maxMealCanReserve;
                Customer.JobId = request.jobId;
                Customer.SettlementId = request.settlementId;
                Customer.DishId = request.dishId;
                Customer.IsLegal = request.isLegal;
                Customer.Code = request.code;
                Customer.Lat = request.lat;
                Customer.Lng = request.lng;
                Customer.AgentFullName = request.agentFullName;
                Customer.CompanyType = request.companyType;
                Customer.BranchForSaturday = request.branchForSaturday;
                Customer.BranchForSunday = request.branchForSunday;
                Customer.BranchForMonday = request.branchForMonday;
                Customer.BranchForTuesday = request.branchForTuesday;
                Customer.BranchForWednesday = request.branchForWednesday;
                Customer.BranchForThursday = request.branchForThursday;
                Customer.BranchForFriday = request.branchForFriday;
                Customer.DeliverFullName = request.deliverFullName;
                Customer.DeliverPhoneNumber = request.deliverPhoneNumber;
                Customer.PriceType = request.priceType;
                Customer.AvragePrice = request.avragePrice;
                Customer.ShippingFee = request.shippingFee;


                foreach (var child in childCustomers)
                {
                    child.ContractStartDate = request.contractStartDate;
                    child.ContractEndDate = request.contractEndDate;
                    if(request.active == false)
                    {
                        child.Active = false;
                    }
                    _NarijeDBContext.Customers.Update(child);
                }

                _NarijeDBContext.Customers.Update(Customer);

                var existingMeals = _NarijeDBContext.CompanyMeal.Where(m => m.CustomerId == Customer.Id);
                var existingAccessories = _NarijeDBContext.AccessoryCompany.Where(a => a.CompanyId == Customer.Id);

                _NarijeDBContext.CompanyMeal.RemoveRange(existingMeals);
                _NarijeDBContext.AccessoryCompany.RemoveRange(existingAccessories);

                if (request.CustomerMeal != null && request.CustomerMeal.Any())
                {
                    var customerMeals = request.CustomerMeal.Select(meal => new CompanyMeal()
                    {
                        CustomerId = Customer.Id,
                        MealId = meal.mealId,
                        MaxReserveTime = meal.maxReserveTime,
                        MaxNumberCanReserve = meal.maxNumberCanReserve,
                        DeliverHour = meal.deliverHour,
                        Active = meal.active,
                        FoodServerNumber = meal.foodServerNumber,
                    }).ToList();

                    await _NarijeDBContext.CompanyMeal.AddRangeAsync(customerMeals);
                }

                if (request.customerAccessory != null && request.customerAccessory.Any())
                {
                    var customerAccessories = request.customerAccessory.Select(accessory => new AccessoryCompany()
                    {
                        CompanyId = Customer.Id,
                        AccessoryId = accessory.accessoryId,
                        Numbers = accessory.numbers,
                        Price = accessory.price
                    }).ToList();

                    await _NarijeDBContext.AccessoryCompany.AddRangeAsync(customerAccessories);
                }

                if (changes.Count > 0)
                {
                    await _logHistoryHelper.AddLogHistoryAsync(
                        "Customer",
                        Customer.Id,
                        EnumLogHistroyAction.update,
                        EnumLogHistorySource.site,
                        JsonSerializer.Serialize(request),
                        false
                    );
                }

                var Result = await _NarijeDBContext.SaveChangesAsync();
                if (Result < 0)
                    return new ApiErrorResponse(StatusCodes.Status405MethodNotAllowed, "اطلاعات ذخیره نشد! دوباره سعی کنید");

                await transaction.CommitAsync();
                return await GetAsync(Customer.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiErrorResponse(StatusCodes.Status405MethodNotAllowed, ex.InnerException?.Message ?? ex.Message);
            }
        }


        private async Task UpdateReservesAndWallet(Reserve reserve, int oldPrice, int newPrice)
        {



            var user = await _NarijeDBContext.Users.FindAsync(reserve.UserId);
            if (user != null)
            {
                long wallet = await GetWalletBalance(_NarijeDBContext, user.Id);
                int priceDiff = Math.Abs(oldPrice - newPrice);
                int operation = oldPrice > newPrice ? (int)EnumWalletOp.Revoke : (int)EnumWalletOp.Debit;
                string msg = newPrice == 0 ? $"عودت وجه به کیف پول حذف غذای '{reserve.Food.Title}'" :
                    operation == (int)EnumWalletOp.Debit
                  ? $"کم کردن وجه از کیف پول بابت تغییر قیمت غذای '{reserve.Food.Title}': {oldPrice} به قیمت: {newPrice}"
                  : $"عودت وجه به کیف پول بابت تغییر قیمت غذای '{reserve.Food.Title}': {oldPrice} به قیمت: {newPrice}";


                long newWalletValue = operation == (int)EnumWalletOp.Debit
                    ? wallet - priceDiff
                    : wallet + priceDiff;

                await ProcessWallet(_NarijeDBContext, user.Id, wallet, priceDiff, operation, newWalletValue, msg);
            }

        }
        private async Task<long> GetWalletBalance(NarijeDBContext dbContext, int userId)
        {


            return await dbContext.Wallets
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Id)
                .Select(w => w.RemValue)
                .FirstOrDefaultAsync();

        }
        private async Task<string> ProcessWallet(NarijeDBContext dbContext, int userId, long wallet, long price, int op, long value, string msg)
        {

            Wallet w = new Wallet()
            {
                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                Op = op,
                Value = price,
                UserId = userId,
                PreValue = wallet,
                RemValue = value,
                Description = msg
            };
            w.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(w);
            await dbContext.Wallets.AddAsync(w);

            WalletPayment wp = new WalletPayment()
            {
                Status = 1,
                UserId = userId,
                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                Op = op,
                Value = price,
                Gateway = (int)EnumGateway.Wallet,
                Wallet = w,
                Description = msg
            };
            await dbContext.WalletPayments.AddAsync(wp);



            return "";
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var customer = await _NarijeDBContext.Customers
                                                 .Include(c => c.Menus)
                                                 .Include(c => c.Reserves)
                                                 .Include(c => c.Users)
                                                 .Include(c => c.FoodPrices)
                                                 .Include(c => c.Invoices)
                                                 .Include(c => c.Surveys)
                                                 .Include(c => c.Credits)
                                                 .Include(c => c.CompanyMeals)
                                                 .Include(c => c.AccessoryCompanies)
                                                 .FirstOrDefaultAsync(c => c.Id == id);

            if (customer is null)
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status404NotFound,
                    _Message: "اطلاعات جهت حذف یافت نشد"
                );

            bool hasChild = await _NarijeDBContext.Customers.AnyAsync(c => c.ParentId == id);
            if (hasChild)
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status405MethodNotAllowed,
                    _Message: "این شرکت دارای شعبه می‌باشد"
                );

            var relatedEntities = new List<string>();

            if (customer.Menus.Any()) relatedEntities.Add("منو");
            if (customer.Reserves.Any()) relatedEntities.Add("رزرو");
            if (customer.Users.Any()) relatedEntities.Add("کاربر");
            if (customer.FoodPrices.Any()) relatedEntities.Add("قیمت غذا");
            if (customer.Invoices.Any()) relatedEntities.Add("فاکتور");
            if (customer.Surveys.Any()) relatedEntities.Add("نظرسنجی");
            if (customer.Credits.Any()) relatedEntities.Add("اعتبار");

            if (relatedEntities.Any())
            {
                string joinedNames = string.Join("، ", relatedEntities);
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status405MethodNotAllowed,
                    _Message: $"امکان حذف وجود ندارد زیرا این شرکت دارای سوابق {joinedNames} می‌باشد"
                );
            }


            if (customer.CompanyMeals.Any())
                _NarijeDBContext.RemoveRange(customer.CompanyMeals);

            if (customer.AccessoryCompanies.Any())
                _NarijeDBContext.RemoveRange(customer.AccessoryCompanies);

            _NarijeDBContext.Customers.Remove(customer);

            var result = await _NarijeDBContext.SaveChangesAsync();

            if (result <= 0)
                return new ApiErrorResponse(
                    _Code: StatusCodes.Status405MethodNotAllowed,
                    _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید"
                );

            return new ApiOkResponse(
                _Message: "SUCCESS",
                _Data: null
            );
        }

        #endregion

        #region EditActiveAsync
        // ------------------
        //  EditActiveAsync
        // ------------------
        public async Task<ApiResponse> EditActiveAsync(int id)
        {
            var Data = await _NarijeDBContext.Customers
                                                  .Where(A => A.Id == id)
                                                  .FirstOrDefaultAsync();
            if (Data is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

         

            if(Data.Active == true)
            {
                var childCustomers = await _NarijeDBContext.Customers
                   .Where(c => c.ParentId == id)
                   .ToListAsync();

                foreach (var child in childCustomers)
                {
                    child.Active = false;
                    _NarijeDBContext.Customers.Update(child);
                }


            }

            Data.Active = !Data.Active;

            _NarijeDBContext.Customers.Update(Data);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(Data.Id);
        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(CustomerCloneRequest request)
        {
            var Customer = await _NarijeDBContext.Customers
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Customer is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion

        #region UpdateCustomerIfTheirExpired
        public async Task UpdateCustomersAsync()
        {
            try
            {
                var customersToUpdate = await _NarijeDBContext.Customers
                .Where(c => c.ContractEndDate < DateTime.Now && c.Active)
               .ToListAsync();

                foreach (var customer in customersToUpdate)
                {
                    customer.Active = false;
                }

                await _NarijeDBContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {

                throw;
            }



        }

        #endregion

        #region ExportBranchServicesAsync
        public async Task<FileContentResult> ExportBranchServicesAsync(DateTime fromData, DateTime toData, int customerId = 0, bool showProductId = false, bool showFoodType = false, bool showFoodGroup = false,
            bool showVat = false, bool showArpa = false, bool showQty = false, bool isFood = false)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Reserve");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.vReserves.Where(c => c.Num != 0 && c.CustomerId == customerId);
            Q = Q.Where(r => r.DateTime.Date >= fromData.Date && r.DateTime.Date <= toData.Date);



            var reserves = await Q.ToListAsync();
            var allMealTitles = _NarijeDBContext.Meal.Select(m => m.Title).ToList();

            if (!reserves.Any())
                throw new Exception("No reserves found.");

            var groupedReserves = reserves
                .GroupBy(r => r.BranchTitle)
                .Select(g => new
                {

                    BranchTitle = g.Key,
                    PeriodStart = Q.Min(r => r.DateTime),
                    PeriodEnd = Q.Max(r => r.DateTime),
                    FoodTotals = g.GroupBy(r => r.FoodTitle)
                                  .ToDictionary(fg => fg.Key, fg => fg.Sum(r => r.Num)),
                    MealTotals = g.GroupBy(r => r.MealTitle)
                                  .ToDictionary(mg => mg.Key, mg => mg.Sum(r => r.Num)),
                    TotalSum = g.Sum(r => r.Num),
                    TotalPriceSum = g.Sum(r => r.Num * r.Price * 10),

                })
                .ToList();

            var groupedReservesCustomer = reserves
                 .GroupBy(r => new { r.DateTime.Date, r.MealType })
                    .OrderBy(g => g.Key.Date).ThenBy(g => g.Key.MealType);

            var allFoodTitle = reserves
                .Select(item => item.FoodTitle)
                .Distinct()
                .ToList();
            var customer = await _NarijeDBContext.Customers.FindAsync(customerId);
            var persianCalendar = new PersianCalendar();
            var periodStart = groupedReserves.Min(g => g.PeriodStart);
            var periodEnd = groupedReserves.Max(g => g.PeriodEnd);
            var shamsiPeriodStart = $"{persianCalendar.GetYear(fromData)}/{persianCalendar.GetMonth(fromData):D2}/{persianCalendar.GetDayOfMonth(fromData):D2}";
            var shamsiPeriodEnd = $"{persianCalendar.GetYear(toData)}/{persianCalendar.GetMonth(toData):D2}/{persianCalendar.GetDayOfMonth(toData):D2}";
            var shamsiDate = $"{persianCalendar.GetYear(DateTime.Now)}/{persianCalendar.GetMonth(DateTime.Now):D2}/{persianCalendar.GetDayOfMonth(DateTime.Now):D2}";
            var fileName = $"خروجی خدمات شعبه ها {shamsiDate}.xlsx";

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Branch Services");
                worksheet.View.RightToLeft = true;
                // Title
                worksheet.Cells[1, 2].Value = $"گزارش برنامه تولید شرکت {customer.Title} :‌ {shamsiPeriodStart} - {shamsiPeriodEnd}";
                worksheet.Cells[1, 2, 1, allFoodTitle.Count + allMealTitles.Count + 4].Merge = true;
                worksheet.Cells[1, 2].Style.Font.Size = 13;
                worksheet.Cells[1, 2].Style.Font.Bold = false;
                worksheet.Cells[1, 2].Style.Font.Color.SetColor(Color.Black);
                worksheet.Cells[1, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                worksheet.Row(1).Height = 25;

                // Header Row
                worksheet.Cells[2, 2].Value = "شعبه خدمات دهنده";

                int colIndex = 3;
                foreach (var foodTitle in allFoodTitle)
                {
                    worksheet.Cells[2, colIndex].Value = foodTitle;
                    colIndex++;
                }

                foreach (var mealTitle in allMealTitles)
                {
                    worksheet.Cells[2, colIndex].Value = mealTitle;
                    colIndex++;
                }

                worksheet.Cells[2, colIndex].Value = "جمع تعداد";
                colIndex++;
                worksheet.Cells[2, colIndex].Value = " جمع قیمت - ریال";

                // Sum Row
                worksheet.Cells[2, 2, 2, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[2, 2, 2, colIndex].Style.Fill.BackgroundColor.SetColor(Color.Orange);
                worksheet.Cells[2, colIndex].Style.Font.Bold = false;

                //  colIndex = 3;
                //  foreach (var foodTitle in allFoodTitle)
                //  {
                //     worksheet.Cells[3, colIndex].Value = groupedReserves.Sum(branch => branch.FoodTotals.ContainsKey(foodTitle) ? branch.FoodTotals[foodTitle] : 0);
                //     colIndex++;
                // }

                //  foreach (var mealTitle in allMealTitles)
                //  {
                //      worksheet.Cells[3, colIndex].Value = groupedReserves.Sum(branch => branch.MealTotals.ContainsKey(mealTitle) ? branch.MealTotals[mealTitle] : 0);
                //      colIndex++;
                //  }

                // worksheet.Cells[3, colIndex].Value = groupedReserves.Sum(branch => branch.TotalSum);
                // colIndex++;
                // worksheet.Cells[3, colIndex].Value = groupedReserves.Sum(branch => branch.TotalPriceSum);

                // Data Rows with Alternating Colors
                int rowIndex = 3;
                var colors = new[] { Color.LightBlue, Color.Yellow, Color.LightGreen, Color.Red, Color.Blue };
                var branchColors = new Dictionary<string, Color>();

                foreach (var branch in groupedReserves)
                {
                    var backgroundColor = colors[(rowIndex - 3) % colors.Length];
                    branchColors[branch.BranchTitle] = backgroundColor;

                    worksheet.Cells[rowIndex, 2].Value = branch.BranchTitle;

                    colIndex = 3;
                    foreach (var foodTitle in allFoodTitle)
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = branch.FoodTotals.ContainsKey(foodTitle) ? branch.FoodTotals[foodTitle] : 0;
                        colIndex++;
                    }

                    foreach (var mealTitle in allMealTitles)
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = branch.MealTotals.ContainsKey(mealTitle) ? branch.MealTotals[mealTitle] : 0;
                        colIndex++;
                    }

                    worksheet.Cells[rowIndex, colIndex].Value = branch.TotalSum;
                    colIndex++;
                    worksheet.Cells[rowIndex, colIndex].Value = branch.TotalPriceSum;

                    // Set background color
                    worksheet.Cells[rowIndex, 2, rowIndex, colIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[rowIndex, 2, rowIndex, colIndex].Style.Fill.BackgroundColor.SetColor(backgroundColor);

                    rowIndex++;
                }


                rowIndex += 4;



                foreach (var group in groupedReservesCustomer)
                {
                    var shamsiDates = $"{persianCalendar.GetYear(group.Key.Date)}/{persianCalendar.GetMonth(group.Key.Date):D2}/{persianCalendar.GetDayOfMonth(group.Key.Date):D2}";

                    var persianCulture = new CultureInfo("fa-IR");
                    var dayName = persianCulture.DateTimeFormat.GetDayName(group.Key.Date.DayOfWeek);

                    var branchId = group.First().BranchId;
                    var backgroundColor = branchColors.ContainsKey(group.First().BranchTitle) ? branchColors[group.First().BranchTitle] : Color.White;

                    var customerMeal = await _NarijeDBContext.CompanyMeal
                        .Where(c => c.CustomerId == customer.Id && c.MealId == group.Key.MealType)
                        .FirstOrDefaultAsync();



                    var colNumber = 1; // Track number of rows per group
                    int maxColumn = 5; // Default max column index to cover dynamically

                    // First row
                    worksheet.Cells[rowIndex, 2].Value = "نام شعبه";
                    worksheet.Cells[rowIndex, 3].Value = customer.Title;
                    worksheet.Cells[rowIndex, 4].Value = "نام کالا";

                    int colIndex1 = 5;
                    foreach (var reserve in group)
                    {
                        worksheet.Cells[rowIndex, colIndex1].Value = reserve.FoodTitle;
                        colIndex1++;
                    }
                    maxColumn = Math.Max(maxColumn, colIndex1 - 1);
                    rowIndex++;

                    // Second row
                    worksheet.Cells[rowIndex, 2].Value = "ساعت تحویل";
                    if (TimeSpan.TryParse(customerMeal.DeliverHour, out var time))
                    {
                        worksheet.Cells[rowIndex, 3].Value = time.ToString(@"hh\:mm");
                    }
                    else
                    {
                        worksheet.Cells[rowIndex, 3].Value = customerMeal.DeliverHour;
                    }

                    if (showProductId)
                    {
                        worksheet.Cells[rowIndex, 4].Value = "شناسه کالا";
                        int colIndex2 = 5;
                        foreach (var reserve in group)
                        {
                            worksheet.Cells[rowIndex, colIndex2].Value = reserve.FoodId;
                            colIndex2++;
                        }
                        maxColumn = Math.Max(maxColumn, colIndex2 - 1);
                        colNumber++;
                    }
                    rowIndex++;

                    // Third row
                    worksheet.Cells[rowIndex, 1].Value = "شعبه خدمات دهنده";
                    worksheet.Cells[rowIndex, 2].Value = "وعده";
                    worksheet.Cells[rowIndex, 3].Value = group.First().MealTitle;

                    if (showQty)
                    {
                        worksheet.Cells[rowIndex, 4].Value = "تعداد سفارش";
                        int colIndex3 = 5;
                        foreach (var reserve in group)
                        {
                            worksheet.Cells[rowIndex, colIndex3].Value = reserve.Num;
                            colIndex3++;
                        }
                        maxColumn = Math.Max(maxColumn, colIndex3 - 1);
                        colNumber++;
                    }
                    rowIndex++;

                    // Fourth row
                    worksheet.Cells[rowIndex, 1].Value = dayName + " " + group.First().BranchTitle;
                    worksheet.Cells[rowIndex, 2].Value = "کد مشتری";
                    worksheet.Cells[rowIndex, 3].Value = customer.Code;

                    if (showFoodType)
                    {
                        worksheet.Cells[rowIndex, 4].Value = "نوع کالا";
                        int colIndex4 = 5;
                        foreach (var reserve in group)
                        {
                            worksheet.Cells[rowIndex, colIndex4].Value = reserve.ProductTypeTitle;
                            colIndex4++;
                        }
                        maxColumn = Math.Max(maxColumn, colIndex4 - 1);
                        colNumber++;
                    }
                    rowIndex++;

                    // Fifth row
                    worksheet.Cells[rowIndex, 2].Value = "تاریخ تحویل";
                    worksheet.Cells[rowIndex, 3].Value = shamsiDates;

                    if (isFood)
                    {
                        worksheet.Cells[rowIndex, 4].Value = "ایا این غذا هست ؟ ";
                        int colIndex5 = 5;
                        foreach (var reserve in group)
                        {
                            worksheet.Cells[rowIndex, colIndex5].Value = reserve.IsFood ? "بلی" : "خیر";
                            colIndex5++;
                        }
                        maxColumn = Math.Max(maxColumn, colIndex5 - 1);
                        colNumber++;
                    }
                    rowIndex++;

                    // Sixth row
                    if (showFoodGroup)
                    {
                        worksheet.Cells[rowIndex, 4].Value = "نام گروه کالا";
                        int colIndex6 = 5;
                        foreach (var reserve in group)
                        {
                            worksheet.Cells[rowIndex, colIndex6].Value = reserve.FoodGroupTitle;
                            colIndex6++;
                        }
                        maxColumn = Math.Max(maxColumn, colIndex6 - 1);
                        colNumber++;
                    }
                    rowIndex++;

                    // Seventh row
                    if (showVat)
                    {
                        worksheet.Cells[rowIndex, 4].Value = "مالیات";
                        int colIndex7 = 5;
                        foreach (var reserve in group)
                        {
                            worksheet.Cells[rowIndex, colIndex7].Value = reserve.FoodVat;
                            colIndex7++;
                        }
                        maxColumn = Math.Max(maxColumn, colIndex7 - 1);
                        colNumber++;
                    }
                    rowIndex++;

                    // Eighth row
                    if (showArpa)
                    {
                        worksheet.Cells[rowIndex, 4].Value = "کد کالا";
                        int colIndex8 = 5;
                        foreach (var reserve in group)
                        {
                            worksheet.Cells[rowIndex, colIndex8].Value = reserve.FoodArpaNumber;
                            colIndex8++;
                        }
                        maxColumn = Math.Max(maxColumn, colIndex8 - 1);
                        colNumber++;
                    }
                    rowIndex++;

                    // Apply background color for all these rows
                    int startRow = rowIndex - 8;
                    int endRow = rowIndex - 1;

                    worksheet.Cells[startRow, 2, endRow, maxColumn].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[startRow, 2, endRow, maxColumn].Style.Fill.BackgroundColor.SetColor(backgroundColor);

                    // Set first column to light gray
                    worksheet.Cells[startRow, 2, endRow, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[startRow, 2, endRow, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                    // Align all cells in the group to the right
                    worksheet.Cells[startRow, 1, endRow, maxColumn].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                    rowIndex += 2; // Leave a gap between groups
                }

                worksheet.Column(1).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Column(1).Style.Fill.BackgroundColor.SetColor(Color.White);
                worksheet.Cells[1, 1, rowIndex, colIndex].Style.Font.Name = "Arial";
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var excelBytes = package.GetAsByteArray();

                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }


        #endregion

        #region CustomerAccessoryExport
        public async Task<FileContentResult> CustomerAccessoryExport(int companyId)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var currentCustomer = await _NarijeDBContext.Customers
                .FirstOrDefaultAsync(c => c.Id == companyId);



            var parentCustomer = await _NarijeDBContext.Customers
                .FirstOrDefaultAsync(c => c.Id == currentCustomer.ParentId);



            var accessoryCompanies = await _NarijeDBContext.AccessoryCompany
                .Where(ac => ac.CompanyId == companyId)
                .Include(ac => ac.Accessory)
                .ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Accessories Report");
                worksheet.View.RightToLeft = true;
                // Header row
                worksheet.Cells[1, 1].Value = "نام شعبه";
                worksheet.Cells[1, 2].Value = currentCustomer.Title;
                worksheet.Cells[1, 3].Value = "نام شرکت";
                worksheet.Cells[1, 4].Value = parentCustomer.Title;
                worksheet.Cells[1, 5].Value = "کد شرکت";
                worksheet.Cells[1, 6].Value = parentCustomer.Code;
                worksheet.Cells[1, 7].Value = "کد شعبه";
                worksheet.Cells[1, 8].Value = currentCustomer.Code;
                using (var headerCells = worksheet.Cells[1, 1, 1, 8])
                {
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    headerCells.Style.Font.Name = "Arial";
                }
                // Accessory table header
                int rowIndex = 3;
                worksheet.Cells[rowIndex, 1].Value = "نام اکسسوری";
                worksheet.Cells[rowIndex, 2].Value = "تعداد";
                worksheet.Cells[rowIndex, 3].Value = "قیمت";
                worksheet.Cells[rowIndex, 4].Value = "قیمت کل";
                using (var tableHeaderCells = worksheet.Cells[rowIndex, 1, rowIndex, 4])
                {
                    tableHeaderCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    tableHeaderCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    tableHeaderCells.Style.Font.Name = "Arial";
                    tableHeaderCells.Style.Font.Bold = true;
                }

                // Add accessory data
                rowIndex++;
                foreach (var accessoryCompany in accessoryCompanies)
                {
                    worksheet.Cells[rowIndex, 1].Value = accessoryCompany.Accessory.Title;
                    worksheet.Cells[rowIndex, 2].Value = accessoryCompany.Numbers;
                    worksheet.Cells[rowIndex, 3].Value = accessoryCompany.Price;
                    worksheet.Cells[rowIndex, 4].Value = accessoryCompany.Price * accessoryCompany.Numbers;
                    using (var dataCells = worksheet.Cells[rowIndex, 1, rowIndex, 4])
                    {
                        dataCells.Style.Font.Name = "Arial";
                    }
                    rowIndex++;
                }

                worksheet.Cells[1, 1, rowIndex - 1, 4].AutoFitColumns();


                // Return the Excel file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"CustomerAccessoryReport_{currentCustomer.Title}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var excelBytes = package.GetAsByteArray();

                return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = fileName
                };
            }
        }

        #endregion

    }
}


