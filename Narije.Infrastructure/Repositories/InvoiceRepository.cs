using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Invoice;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Narije.Core.DTOs.Enum;
using Narije.Core.DTOs.Admin;

namespace Narije.Infrastructure.Repositories
{
    public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public InvoiceRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        {
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            try
            {

                var invoice = await _NarijeDBContext.Invoices
                                        .Where(A => A.Id == id)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            serial = A.Serial,
                                            customerId = A.CustomerId,
                                            customer = A.Customer.Title,
                                            qty = A.Qty,
                                            totalPrice = A.TotalPrice * 10,
                                            hasVat = A.HasVat,
                                            vat = A.Vat * 10,
                                            finalPrice = A.FinalPrice * 10,
                                            description = A.Description,
                                            datetime = A.DateTime,
                                            fromDate = A.FromDate,
                                            toDate = A.ToDate,
                                            updatedAt = A.UpdatedAt,
                                            transportFee = A.TransportFee,
                                            payType = A.PayType,
                                            transportQty = A.TransportQty
                                        })
                                        .FirstOrDefaultAsync();

                var cusotmerIds = _NarijeDBContext.Customers
                                        .Where(A => A.Id == invoice.customerId || A.ParentId == invoice.customerId)
                                        .Select(A => A.Id)
                                        .ToList();
                bool WithCustomerName = false;
                if (cusotmerIds.Count > 1)
                    WithCustomerName = true;


                var D = _NarijeDBContext.Reserves
                                         .Where(A => cusotmerIds.Contains(A.CustomerId) && A.DateTime.Date >= invoice.fromDate.Date
                                                            && A.DateTime.Date <= invoice.toDate.Date && A.State != (int)EnumReserveState.canceled)
                                         .Select(A => A.DateTime.Date)
                                         .OrderBy(A => A.Date)
                                         .Distinct();


                var dates = await D.ToListAsync();

                var reserves = await _NarijeDBContext.Reserves
                                         .Where(A => cusotmerIds.Contains(A.CustomerId) && dates.Contains(A.DateTime.Date) && A.State != (int)EnumReserveState.canceled)
                                         .Select(A => new
                                         {
                                             id = A.Id,
                                             dateTime = A.DateTime.Date,
                                             customer = A.Customer.Title,
                                             customerId = A.CustomerId,
                                             state = A.State,
                                             foodId = A.FoodId,
                                             food = A.Food.Title,
                                             price = A.Price,
                                             qty = A.Num,
                                             vat = A.Food.Vat
                                         }).ToListAsync();

                int row = 1;
                bool first = true;
                List<InvoiceDetailResponse> details = new();
                foreach (var date in dates)
                {
                    var foods = reserves
                        .Where(A => A.dateTime.Date == date)
                        .Select(A => new
                        {
                            A.foodId,
                            A.food,
                            A.customer,
                            A.customerId,
                            price = A.price * 10,
                            vat = A.vat
                        }).Distinct().ToList();
                    first = true;
                    foreach (var food in foods)
                    {
                        var qty = reserves.Where(A => A.dateTime.Date == date && A.foodId == food.foodId && A.customerId == food.customerId && A.price == food.price / 10).Sum(A => A.qty);
                        details.Add(new InvoiceDetailResponse()
                        {
                            row = row,
                            datetime = date.ToString("yyyy-MM-dd"),
                            foodId = food.foodId.ToString(),
                            food = food.food + (!WithCustomerName ? "" : $" ({food.customer})"),
                            price = food.price,
                            qty = qty,
                            type = 0,
                            totalPrice = qty * food.price,
                            vat = food.vat == null ? (long)((qty * food.price) * 0.09) : (long)((qty * food.price) * ((double)food.vat / 100)) //invoice.hasVat ? (long)((qty * food.price) * 0.09) : 0
                        });
                        first = false;
                        row++;
                    }

                }
                foreach (var item in details)
                    item.finalPrice = item.vat + item.totalPrice;

                var invoicedetails = await _NarijeDBContext.InvoiceDetails.Where(A => A.InvoiceId == invoice.id).ToListAsync();
                first = true;
                foreach (var item in invoicedetails)
                {
                    details.Add(new InvoiceDetailResponse()
                    {
                        row = row,
                        datetime = invoice.datetime.ToString("yyyy-MM-dd"),
                        foodId = item.FoodId.ToString(),
                        food = item.Food.Title,// + (!WithCustomerName ? "" : $" ({item.Invoice.Customer.Title})"),
                        price = item.Price * 10,
                        qty = item.Qty,
                        totalPrice = item.TotalPrice * 10,
                        type = 1,
                        vat = item.Vat * 10,
                        finalPrice = item.FinalPrice * 10
                    });
                    row++;
                    first = false;
                }

                row = 1;
                details = details.OrderBy(A => A.datetime).ToList();
                string lastdate = "";
                foreach (var item in details)
                {
                    item.row = row++;
                    if (lastdate == item.datetime)
                        item.datetime = "";
                    lastdate = item.datetime;

                }

                if (invoice.transportFee > 0)
                {
                    details.Add(new InvoiceDetailResponse()
                    {
                        row = row,
                        datetime = "",
                        foodId = "-",
                        food = "هزینه حمل",
                        price = invoice.transportFee * 10,
                        qty = invoice.transportQty,
                        totalPrice = (invoice.transportFee * invoice.transportQty) * 10,
                        type = 0,
                        vat = 0,
                        finalPrice = (invoice.transportFee * invoice.transportQty) * 10
                    });
                }

                var company = await _NarijeDBContext.Settings.Select(A => new
                {
                    companyName = A.CompanyName,
                    tel = A.Tel,
                    address = A.Address,
                    city = A.CityId == null ? "" : A.City.Title,
                    province = A.ProvinceId == null ? "" : A.Province.Title,
                    economicCode = A.EconomicCode,
                    regNumber = A.RegNumber,
                    nationalId = A.NationalId,
                    postalCode = A.PostalCode,
                    logo = A.CompanyGalleryId
                }).FirstOrDefaultAsync();

                var customer = await _NarijeDBContext.Customers
                    .Where(A => A.Id == invoice.customerId)
                    .Select(A => new
                    {
                        title = A.Title,
                        tel = A.Tel,
                        address = A.Address,
                        city = A.CityId == null ? "" : A.City.Title,
                        province = A.ProvinceId == null ? "" : A.Province.Title,
                        economicCode = A.EconomicCode,
                        regNumber = A.RegNumber,
                        nationalId = A.NationalId,
                        postalCode = A.PostalCode
                    }).FirstOrDefaultAsync();

                return new ApiOkResponse(_Message: "SUCCESS", _Data: new
                {
                    invoice,
                    details,
                    company,
                    customer
                });
            }
            catch (Exception Ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "خطایی  اتفاق افتاده است");
            }

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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "Invoice");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.Invoices
                        .ProjectTo<InvoiceResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var Invoices = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: Invoices.Data, _Meta: Invoices.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(InvoiceInsertRequest request)
        {
            try
            {
                var customer = await _NarijeDBContext.Customers.Where(A => A.Id == request.customerId)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                if (customer is null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "فاکتور یافت نشد");

                DateTime LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

                var inv = new Invoice()
                {
                    Description = request.description,
                    Serial = request.serial,
                    DateTime = LocalDate,
                    UpdatedAt = LocalDate,
                    CustomerId = request.customerId,
                    HasVat = request.hasVat,
                    FinalPrice = 0,
                    FromDate = request.fromDate,
                    ToDate = request.toDate,
                    TransportQty = request.transportQty,
                    Qty = 0,
                    Vat = 0,
                    TotalPrice = 0,
                    TransportFee = request.transportFee,
                    PayType = request.payType
                };

                var cusotmerIds = _NarijeDBContext.Customers
                                        .Where(A => A.Id == inv.CustomerId || A.ParentId == inv.CustomerId)
                                        .Select(A => A.Id)
                                        .ToList();

                var foods = await _NarijeDBContext.Foods.Select(A => new
                {
                    Id = A.Id,
                    A.Vat,
                    Price = A.FoodPrices.Where(B => B.CustomerId == inv.CustomerId).Select(A => A.EchoPrice).FirstOrDefault() == 0 ?
                            A.EchoPrice : A.FoodPrices.Where(B => B.CustomerId == inv.CustomerId).Select(A => A.EchoPrice).FirstOrDefault()
                }).ToListAsync();

                List<InvoiceDetail> details = new();
                foreach (var item in request.details)
                {
                    var food = foods.Where(A => A.Id == item.foodId).FirstOrDefault();

                    if (food != null)
                    {
                        var detail = new InvoiceDetail()
                        {
                            Invoice = inv,
                            FoodId = item.foodId,
                            Qty = item.qty,
                            Price = food.Price,
                            TotalPrice = (food.Price) * item.qty,
                            Vat = 0
                        };
                        if (food.Vat == null)
                            detail.Vat = (int)(detail.TotalPrice * 0.09);
                        else
                            detail.Vat = (int)(detail.TotalPrice * ((double)food.Vat / 100));

                        //if (inv.HasVat)
                        //    detail.Vat = (int)(detail.TotalPrice * 0.09);
                        detail.FinalPrice = detail.Vat + detail.TotalPrice;

                        details.Add(detail);
                    }
                }

                foreach (var item in details)
                {
                    inv.Qty += item.Qty;
                    inv.Vat += item.Vat;
                    inv.TotalPrice += item.TotalPrice;
                    inv.FinalPrice += item.FinalPrice;
                }

                var reserve = await _NarijeDBContext.Reserves
                                         .Where(A => cusotmerIds.Contains(A.CustomerId) &&
                                                        A.DateTime.Date >= inv.FromDate.Date && A.DateTime.Date <= inv.ToDate.Date &&
                                                        A.State != (int)EnumReserveState.canceled)
                                         .Select(A => new
                                         {
                                             foodId = A.FoodId,
                                             qty = A.Num,
                                             price = A.Price,
                                             total = A.Num * A.Price,
                                             vat = A.Food.Vat
                                         }).ToListAsync();
                long price = 0;
                int qty = 0;
                long vat = 0;
                foreach (var item in reserve)
                {
                    price += item.total;
                    qty += item.qty;
                    vat += item.vat == null ? (long)(item.total * 0.09) : (long)(item.total * ((double)item.vat / 100));
                }
                inv.Qty += qty;
                inv.TotalPrice += price;
                //var vat = (long)(price * 0.09);
                //if (!inv.HasVat)
                //    vat = 0;
                inv.Vat += vat;
                inv.FinalPrice += price + vat;

                if (inv.TransportFee > 0)
                {
                    inv.TotalPrice += inv.TransportFee * inv.TransportQty;
                    inv.FinalPrice += inv.TransportFee * inv.TransportQty;
                }

                await _NarijeDBContext.Invoices.AddAsync(inv);
                await _NarijeDBContext.InvoiceDetails.AddRangeAsync(details);
                await _NarijeDBContext.SaveChangesAsync();

                var result = await _NarijeDBContext.Invoices
                                        .Where(A => A.Id == inv.Id)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            serial = A.Serial,
                                            customerId = A.CustomerId,
                                            customer = A.Customer.Title,
                                            qty = A.Qty,
                                            totalPrice = A.TotalPrice,
                                            hasVat = A.HasVat,
                                            vat = A.Vat,
                                            finalPrice = A.FinalPrice,
                                            description = A.Description,
                                            datetime = A.DateTime,
                                            fromDate = A.FromDate,
                                            toDate = A.ToDate,
                                            updatedAt = A.UpdatedAt,
                                            transportFee = A.TransportFee,
                                            payType = A.PayType
                                        })
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

                return new ApiOkResponse(_Message: "SUCCEED", _Data: result);
            }
            catch (Exception Ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "خطایی  اتفاق افتاده است");
            }
        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(InvoiceEditRequest request)
        {
            try
            {
                var inv = await _NarijeDBContext.Invoices.Where(A => A.Id == request.id)
                                        .FirstOrDefaultAsync();
                if (inv is null)
                    return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "فاکتور یافت نشد");

                DateTime LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

                inv.Serial = request.serial;
                inv.Description = request.description;
                inv.HasVat = request.hasVat;
                inv.FromDate = request.fromDate;
                inv.ToDate = request.toDate;
                inv.Serial = request.serial;
                inv.UpdatedAt = LocalDate;
                inv.TransportFee = request.transportFee;
                inv.PayType = request.payType;
                inv.TransportQty = request.transportQty;
                inv.TotalPrice = 0;
                inv.Vat = 0;
                inv.FinalPrice = 0;
                inv.Qty = 0;

                var cusotmerIds = _NarijeDBContext.Customers
                                        .Where(A => A.Id == inv.CustomerId || A.ParentId == inv.CustomerId)
                                        .Select(A => A.Id)
                                        .ToList();

                var MustDelete = inv.InvoiceDetails.ToList();
                _NarijeDBContext.InvoiceDetails.RemoveRange(MustDelete);

                var foods = await _NarijeDBContext.Foods.Select(A => new
                {
                    Id = A.Id,
                    A.Vat,
                    Price = A.FoodPrices.Where(B => B.CustomerId == inv.CustomerId).Select(A => A.EchoPrice).FirstOrDefault() == 0 ?
                            A.EchoPrice : A.FoodPrices.Where(B => B.CustomerId == inv.CustomerId).Select(A => A.EchoPrice).FirstOrDefault()
                }).ToListAsync();

                List<InvoiceDetail> details = new();
                foreach (var item in request.details)
                {
                    var food = foods.Where(A => A.Id == item.foodId).FirstOrDefault();

                    if (food != null)
                    {
                        var detail = new InvoiceDetail()
                        {
                            Invoice = inv,
                            FoodId = item.foodId,
                            Qty = item.qty,
                            Price = food.Price,
                            TotalPrice = food.Price * item.qty,
                            Vat = 0
                        };

                        if (food.Vat == null)
                            detail.Vat = (int)(detail.TotalPrice * 0.09);
                        else
                            detail.Vat = (int)(detail.TotalPrice * ((double)food.Vat / 100));

                        //if (inv.HasVat)
                        //    detail.Vat = (int)(detail.TotalPrice * 0.09);
                        detail.FinalPrice = detail.Vat + detail.TotalPrice;

                        details.Add(detail);
                    }
                }

                foreach (var item in details)
                {
                    inv.Qty += item.Qty;
                    inv.Vat += item.Vat;
                    inv.TotalPrice += item.TotalPrice;
                    inv.FinalPrice += item.FinalPrice;
                }

                var reserve = await _NarijeDBContext.Reserves
                                         .Where(A => cusotmerIds.Contains(A.CustomerId) &&
                                                        A.DateTime.Date >= inv.FromDate.Date && A.DateTime.Date <= inv.ToDate.Date &&
                                                        A.State != (int)EnumReserveState.canceled)
                                         .Select(A => new
                                         {
                                             foodId = A.FoodId,
                                             qty = A.Num,
                                             price = A.Price,
                                             total = A.Num * A.Price,
                                             vat = A.Food.Vat
                                         }).ToListAsync();
                long price = 0;
                int qty = 0;
                long vat = 0;
                foreach (var item in reserve)
                {
                    price += item.total;
                    qty += item.qty;
                    vat += item.vat == null ? (long)(item.total * 0.09) : (long)(item.total * ((double)item.vat / 100));
                }
                inv.Qty += qty;
                inv.TotalPrice += price;

                //var vat = (long)(price * 0.09);
                //if (!inv.HasVat)
                //    vat = 0;
                inv.Vat += vat;
                inv.FinalPrice += price + vat;


                if (inv.TransportFee > 0)
                {
                    inv.TotalPrice += inv.TransportFee * inv.TransportQty;
                    inv.FinalPrice += inv.TransportFee * inv.TransportQty;
                }

                _NarijeDBContext.Invoices.Update(inv);
                await _NarijeDBContext.InvoiceDetails.AddRangeAsync(details);
                await _NarijeDBContext.SaveChangesAsync();

                var result = await _NarijeDBContext.Invoices
                                        .Where(A => A.Id == inv.Id)
                                        .Select(A => new
                                        {
                                            id = A.Id,
                                            serial = A.Serial,
                                            customerId = A.CustomerId,
                                            customer = A.Customer.Title,
                                            qty = A.Qty,
                                            totalPrice = A.TotalPrice,
                                            hasVat = A.HasVat,
                                            vat = A.Vat,
                                            finalPrice = A.FinalPrice,
                                            description = A.Description,
                                            datetime = A.DateTime,
                                            fromDate = A.FromDate,
                                            toDate = A.ToDate,
                                            updatedAt = A.UpdatedAt,
                                            transportFee = A.TransportFee,
                                            payType = A.PayType
                                        })
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();

                return new ApiOkResponse(_Message: "SUCCEED", _Data: result);
            }
            catch (Exception Ex)
            {
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "خطایی  اتفاق افتاده است");
            }

        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "فاکتور قابل حذف نیست");

            var Invoice = await _NarijeDBContext.Invoices
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (Invoice is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.Invoices.Remove(Invoice);

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
        public async Task<ApiResponse> CloneAsync(InvoiceCloneRequest request)
        {
            var Invoice = await _NarijeDBContext.Invoices
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (Invoice is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


