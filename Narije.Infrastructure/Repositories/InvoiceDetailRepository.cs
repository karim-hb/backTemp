using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.InvoiceDetail;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;

namespace Narije.Infrastructure.Repositories
{
    public class InvoiceDetailRepository : BaseRepository<InvoiceDetail>, IInvoiceDetailRepository
    {

        // ------------------
        // Constructor
        // ------------------
        public InvoiceDetailRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        { 
        }

        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var InvoiceDetail = await _NarijeDBContext.InvoiceDetails
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<InvoiceDetailResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: InvoiceDetail);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "InvoiceDetail");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.InvoiceDetails
                        .ProjectTo<InvoiceDetailResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var InvoiceDetails = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            return new ApiOkResponse(_Message: "SUCCESS", _Data: InvoiceDetails.Data, _Meta: InvoiceDetails.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(InvoiceDetailInsertRequest request)
        {
            var InvoiceDetail = new InvoiceDetail()
            {
                InvoiceId = request.invoiceId,
                FoodId = request.foodId,
                Qty = request.qty,
                Price = request.price,
                TotalPrice = request.totalPrice,
                Vat = request.vat,
                FinalPrice = request.finalPrice,

            };


            await _NarijeDBContext.InvoiceDetails.AddAsync(InvoiceDetail);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(InvoiceDetail.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(InvoiceDetailEditRequest request)
        {
            var InvoiceDetail = await _NarijeDBContext.InvoiceDetails
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (InvoiceDetail is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            InvoiceDetail.InvoiceId = request.invoiceId;
            InvoiceDetail.FoodId = request.foodId;
            InvoiceDetail.Qty = request.qty;
            InvoiceDetail.Price = request.price;
            InvoiceDetail.TotalPrice = request.totalPrice;
            InvoiceDetail.Vat = request.vat;
            InvoiceDetail.FinalPrice = request.finalPrice;



            _NarijeDBContext.InvoiceDetails.Update(InvoiceDetail);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(InvoiceDetail.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            var InvoiceDetail = await _NarijeDBContext.InvoiceDetails
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (InvoiceDetail is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.InvoiceDetails.Remove(InvoiceDetail);

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
        public async Task<ApiResponse> CloneAsync(InvoiceDetailCloneRequest request)
        {
            var InvoiceDetail = await _NarijeDBContext.InvoiceDetails
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (InvoiceDetail is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion
    }
}


