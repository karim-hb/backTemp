using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.WalletPayment;
using Narije.Core.Entities;
using Narije.Core.Interfaces;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;
using Narije.Infrastructure.Helpers;
using System.Collections.Generic;
using Narije.Core.DTOs.Enum;
using System.Security.Claims;
using Narije.Core.DTOs.ViewModels.Export;
using System.Web;
using TM.Core.DTOs.Enum;
using TM.Infrastructure.Helpers;

namespace Narije.Infrastructure.Repositories
{
    public class WalletPaymentRepository : BaseRepository<WalletPayment>, IWalletPaymentRepository
    {
        // ------------------
        // Constructor
        // ------------------
        public WalletPaymentRepository(IConfiguration _IConfiguration, IHttpContextAccessor _IHttpContextAccessor, NarijeDBContext _NarijeDBContext, IMapper _IMapper) :
            base(_IConfiguration: _IConfiguration, _IHttpContextAccessor: _IHttpContextAccessor, _NarijeDBContext: _NarijeDBContext, _IMapper: _IMapper)
        { 
        }


        #region GetAsync
        // ------------------
        //  GetAsync
        // ------------------
        public async Task<ApiResponse> GetAsync(int id)
        {
            var WalletPayment = await _NarijeDBContext.WalletPayments
                                                 .Where(A => A.Id == id)
                                                 .ProjectTo<WalletPaymentResponse>(_IMapper.ConfigurationProvider)
                                                 .FirstOrDefaultAsync();

            var Users = await _NarijeDBContext.Users
                                .Where(A => A.Id == WalletPayment.applicantId || A.Id == WalletPayment.applierId)
                                .Select(A => new
                                {
                                    A.Id,
                                    UserName = A.Fname + " " + A.Lname
                                })
                                .ToListAsync();

            WalletPayment.applicant = Users.Where(A => A.Id == WalletPayment.applicantId).Select(A => A.UserName).FirstOrDefault();
            WalletPayment.applier = Users.Where(A => A.Id == WalletPayment.applierId).Select(A => A.UserName).FirstOrDefault();

            return new ApiOkResponse(_Message: "SUCCEED", _Data: WalletPayment);
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

            var header = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "WalletPayment");
            var query = FilterHelper.GetQuery(header, _IHttpContextAccessor);

            var Q = _NarijeDBContext.WalletPayments
                        //.Where(A => A.Op == (int)EnumWalletOp.Refund || A.Op == (int)EnumWalletOp.AdminCredit)
                        .ProjectTo<WalletPaymentResponse>(_IMapper.ConfigurationProvider);

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var WalletPayments = await Q.GetPaged(Page: page.Value, Limit: limit.Value);

            var ids = WalletPayments.Data.Select(A => A.applicantId).ToList();
            ids.AddRange(WalletPayments.Data.Select(A => A.applierId).ToList());

            var Users = await _NarijeDBContext.Users
                                .Where(A => ids.Contains(A.Id))
                                .Select(A => new
                                {
                                    A.Id,
                                    UserName = A.Fname + " " + A.Lname
                                })
                                .ToListAsync();

            foreach(var item in WalletPayments.Data)
            {
                item.applicant = Users.Where(A => A.Id == item.applicantId).Select(A => A.UserName).FirstOrDefault();
                item.applier = Users.Where(A => A.Id == item.applierId).Select(A => A.UserName).FirstOrDefault();
            }

            return new ApiOkResponse(_Message: "SUCCESS", _Data: WalletPayments.Data, _Meta: WalletPayments.Meta, _Header: header);

        }
        #endregion

        #region InsertAsync
        // ------------------
        //  InsertAsync
        // ------------------
        public async Task<ApiResponse> InsertAsync(WalletPaymentInsertRequest request)
        {
            if((request.op != (int)EnumWalletOp.Refund) && (request.op != (int)EnumWalletOp.AdminCredit))
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "عملکرد انتخابی مجاز نیست");

            var user = await _NarijeDBContext.Users.Where(A => A.Id == request.userId).FirstOrDefaultAsync();

            if (user is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات مشتری وجود ندارد");

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;

            var WalletPayment = new WalletPayment()
            {
                Op = request.op, 
                UserId = request.userId,
                Value = request.value,
                WalletId = null,
                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                Status = 0,
                RefNumber = request.refNumber,
                Result = request.result,
                Pan = request.pan,
                ConsumeCode = null,
                Bank = request.bank,
                AccountNumber = request.accountNumber,
                Description = request.description,
                Gateway = (int)EnumGateway.Wallet,
                ApplicantId = Convert.ToInt32(Identity.FindFirst("Id").Value)
         };

            WalletPayment.GalleryId = await GalleryHelper.AddFromGallery(_NarijeDBContext, request.fromGallery);
            if (request.files != null)
            {
                var k = await GalleryHelper.AddToGallery(_NarijeDBContext, "WalletPayment", request.files.FirstOrDefault());
                if (k > 0)
                    WalletPayment.GalleryId = k;
            }

            await _NarijeDBContext.WalletPayments.AddAsync(WalletPayment);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(WalletPayment.Id);

        }
        #endregion

        #region EditAsync
        // ------------------
        //  EditAsync
        // ------------------
        public async Task<ApiResponse> EditAsync(WalletPaymentEditRequest request)
        {
            var WalletPayment = await _NarijeDBContext.WalletPayments
                                                  .Where(A => A.Id == request.id)
                                                  .FirstOrDefaultAsync();
            if (WalletPayment is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            if ((request.op != (int)EnumWalletOp.Refund) && (request.op != (int)EnumWalletOp.AdminCredit))
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "عملکرد انتخابی مجاز نیست");

            if (WalletPayment.Status != 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "امکان ویرایش درخواست تعیین تکلیف شده وجود ندارد");

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;

            WalletPayment.ApplicantId = Convert.ToInt32(Identity.FindFirst("Id").Value);
            WalletPayment.Op = request.op;
            WalletPayment.UserId = request.userId;
            WalletPayment.Value = request.value;
            WalletPayment.Status = 0;
            WalletPayment.RefNumber = request.refNumber;
            WalletPayment.Result = request.result;
            WalletPayment.Pan = request.pan;
            WalletPayment.ConsumeCode = null;
            WalletPayment.Bank = request.bank;
            WalletPayment.AccountNumber = request.accountNumber;
            WalletPayment.Description = request.description;
            WalletPayment.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

            if (request.files != null)
                WalletPayment.GalleryId = await GalleryHelper.EditGallery(_NarijeDBContext, WalletPayment.GalleryId, "WalletPayment", request.files.FirstOrDefault());

            _NarijeDBContext.WalletPayments.Update(WalletPayment);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return await GetAsync(WalletPayment.Id);
        }
        #endregion

        #region EditStateAsync
        // ------------------
        //  EditStateAsync
        // ------------------
        public async Task<ApiResponse> EditStateAsync(int id, int state)
        {
            var WalletPayment = await _NarijeDBContext.WalletPayments
                                                  .Where(A => A.Id == id)
                                                  .FirstOrDefaultAsync();
            if (WalletPayment is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت ویرایش یافت نشد");

            if ((state != (int)EnumWalletState.Accepted) && (state != (int)EnumWalletState.Rejected))
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "عملکرد انتخابی مجاز نیست");

            if (WalletPayment.Status != 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "درخواست قبلا تعیین تکلیف شده است");

           // CRC32 crc32 = new CRC32();
           // crc32.AddData($"{WalletPayment.DateTime.ToString("yyy/mm/dd HH:MM")}+{WalletPayment.Id}*{WalletPayment.Value}#{WalletPayment.UserId}");
           // var crc = crc32.Value.ToString("X5");
           // string gcode = "";
           // for (int i = 0; i < crc.Length; i++)
           //     if ((crc[i] <= '0') || (crc[i] >= '9'))
           //         gcode += "8";
           //     else
           //         gcode += crc[i];
           // if (!gcode.Equals(code))
            //    return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "کد صحت سنجی صحیح نمی باشد");

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;

            if (state == (int)EnumWalletState.Accepted)
            {
                var lastwallet = await _NarijeDBContext.Wallets
                                        .Where(A => A.UserId == WalletPayment.UserId)
                                        .OrderByDescending(A => A.Id)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                long PreValue = 0;
                if (lastwallet != null)
                    PreValue = lastwallet.RemValue;

                var wallet = new Wallet()
                {
                    UserId = WalletPayment.UserId,
                    DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                    Value = WalletPayment.Value,
                    PreValue = PreValue,
                };

                switch (WalletPayment.Op)
                {
                    case (int)EnumWalletOp.Refund:
                        wallet.Op = (int)EnumWalletOp.Refund;
                        if(PreValue < WalletPayment.Value)
                            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "موجودی کیف پول برای عوت وجه کافی نیست");
                        wallet.RemValue = PreValue - wallet.Value;
                        break;
                    case (int)EnumWalletOp.AdminCredit:
                        wallet.Op = (int)EnumWalletOp.AdminCredit;
                        wallet.RemValue = PreValue + wallet.Value;
                        break;
                }

                wallet.Opkey = SecurityHelper.WalletGenerateKey(wallet);
                await _NarijeDBContext.Wallets.AddAsync(wallet);
                WalletPayment.Wallet = wallet;
            }

            WalletPayment.ApplierId = Convert.ToInt32(Identity.FindFirst("Id").Value);
            WalletPayment.Status = state;
            WalletPayment.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

            _NarijeDBContext.WalletPayments.Update(WalletPayment);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if (Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");
            else
            {
                /*
                SMSHelper sms = new();
                switch (WalletPayment.Op)
                {
                    case (int)EnumWalletOp.Refund:
                        await sms.SendSMSMessage(_IConfiguration, _NarijeDBContext, (int)EnumSMSTarget.WalletRefund, WalletPayment.User, null, WalletPayment, "", 0, null, MakeJob: true);
                        break;
                    case (int)EnumWalletOp.AdminCredit:
                        await sms.SendSMSMessage(_IConfiguration, _NarijeDBContext, (int)EnumSMSTarget.WalletAdminCharged, WalletPayment.User, null, WalletPayment, "", 0, null, MakeJob: true);
                        break;
                }
                */
            }

            return await GetAsync(WalletPayment.Id);
        }
        #endregion

        #region DeleteAsync
        // ------------------
        //  DeleteAsync
        // ------------------
        public async Task<ApiResponse> DeleteAsync(int id)
        {
            return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "امکان حذف درخواست وجود ندارد");
            
            /*
            var WalletPayment = await _NarijeDBContext.WalletPayments
                                              .Where(A => A.Id == id)
                                              .FirstOrDefaultAsync();
            if (WalletPayment is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت حذف یافت نشد");

            _NarijeDBContext.WalletPayments.Remove(WalletPayment);

            var Result = await _NarijeDBContext.SaveChangesAsync();

            if(Result < 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "اطلاعات ذخیره نشد! دوباره سعی کنید");

            return new ApiOkResponse(_Message: "SUCCESS", _Data: null);
            */

        }
        #endregion

        #region CloneAsync
        // ------------------
        //  CloneAsync
        // ------------------
        /*
        public async Task<ApiResponse> CloneAsync(WalletPaymentCloneRequest request)
        {
            var WalletPayment = await _NarijeDBContext.WalletPayments
                                                 .Where(A => A.Id == request.iId)
                                                 .FirstOrDefaultAsync();
            if (WalletPayment is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات جهت کپی یافت نشد");
        }
        */
        #endregion

        #region Export
        public async Task<ApiResponse> ExportAsync()
        {
            var dbheader = await HeaderHelper.GetHeader(_NarijeDBContext, _IHttpContextAccessor, "WalletPayment", true);
            var query = FilterHelper.GetQuery(dbheader, _IHttpContextAccessor);

            ExportResponse result = new()
            {
                header = new(),
                body = new()
            };

            List<FieldResponse> headers = new();

            result.header = dbheader.Select(A => A.title).ToList();

            var Q = _NarijeDBContext.WalletPayments
                        //.Where(A => A.Op == (int)EnumWalletOp.Refund || A.Op == (int)EnumWalletOp.AdminCredit)
                        .ProjectTo<WalletPaymentResponse>(_IMapper.ConfigurationProvider);

            var Param = HttpUtility.ParseQueryString(_IHttpContextAccessor.HttpContext.Request.QueryString.ToString());
            var key = Param.AllKeys.Where(A => A.Equals("ids")).FirstOrDefault();
            if (key != null)
            {
                var ids = Param[key.ToString()];
                if (ids != "")
                {
                    int[] nids = ids.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    Q = Q.Where(A => nids.Contains(A.id));
                }
            }

            Q = Q.QueryDynamic(query.Search, query.Filter).OrderDynamic(query.Sort);

            var WalletPayments = await Q.ToListAsync();

            var wp = WalletPayments.Select(A => A.applicantId).ToList();
            wp.AddRange(WalletPayments.Select(A => A.applierId).ToList());

            var Users = await _NarijeDBContext.Users
                                .Where(A => wp.Contains(A.Id))
                                .Select(A => new
                                {
                                    A.Id,
                                    UserName = A.Fname + " " + A.Lname
                                })
                                .ToListAsync();

            foreach (var item in WalletPayments)
            {
                item.applicant = Users.Where(A => A.Id == item.applicantId).Select(A => A.UserName).FirstOrDefault();
                item.applier = Users.Where(A => A.Id == item.applierId).Select(A => A.UserName).FirstOrDefault();
            }

            var data = WalletPayments.ToList<object>();


            result.body = ExportHelper.MakeResult(data, dbheader, false);

            return new ApiOkResponse(_Message: "SUCCEED", _Data: result);

        }
        #endregion

        #region RecheckBankTransactionAsync
        /*
        public async Task<OrderBankInfoResponse> RecheckBankTransactionAsync(int wpId)
        {
            var wp = await _NarijeDBContext.WalletPayments
                                     .Where(A => A.Id == wpId)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();

            OrderBankInfoResponse info = new();
            info.Message = null;

            if (wp == null)
            {
                info.Message = "اطلاعات تراکنش یافت نشد";
                return info;
            }

            if (wp.Op != (int)EnumWalletOp.Credit)
            {
                info.Message = "تراکنش مربوط به افزایش اعتبار نمی باشد";
                return info;
            }

            switch (wp.Gateway)
            {
                case (int)EnumGateway.CityBank:
                case (int)EnumGateway.Hybrid:
                    info.Gateway = (int)EnumGateway.CityBank;
                    info.token = wp.Id.ToString();
                    break;
                case (int)EnumGateway.Mellat:
                case (int)EnumGateway.HybridMellat:
                    info.Gateway = (int)EnumGateway.Mellat;
                    info.token = wp.RefNumber.ToString();
                    break;
                case (int)EnumGateway.Ayandeh:
                case (int)EnumGateway.TwoWallets:
                    info.Gateway = (int)EnumGateway.Ayandeh;
                    var data = await _NarijeDBContext.EFardaTempDatas
                                   .Where(A => A.WalletId == wp.Id)
                                   .FirstOrDefaultAsync();
                    if (data == null)
                    {
                        info.Message = "توکن تراکنش در بانک اطلاعاتی یافت نشد";
                        return info;
                    }
                    info.token = data.Token;
                    break;
                default:
                    info.Message = "تراکنش از طریق بانک صورت نگرفته است";
                    return info;

            }

            return info;
        }
        */
        #endregion

        #region EditStateCodeAsync
        // ------------------
        //  EditStateCodeAsync
        // ------------------
        public async Task<ApiResponse> EditStateCodeAsync(int id)
        {
            var WalletPayment = await _NarijeDBContext.WalletPayments
                                                  .Where(A => A.Id == id)
                                                  .FirstOrDefaultAsync();
            if (WalletPayment is null)
                return new ApiErrorResponse(_Code: StatusCodes.Status404NotFound, _Message: "اطلاعات درخواست یافت نشد");

            if (WalletPayment.Status != 0)
                return new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: "درخواست قبلا تعیین تکلیف شده است");

            CRC32 crc32 = new CRC32();
            crc32.AddData($"{WalletPayment.DateTime.ToString("yyy/mm/dd HH:MM")}+{WalletPayment.Id}*{WalletPayment.Value}#{WalletPayment.UserId}");
            var crc = crc32.Value.ToString("X5");
            string code = "";
            for (int i = 0; i < crc.Length; i++)
                if ((crc[i] <= '0') || (crc[i] >= '9'))
                    code += "8";
                else
                    code += crc[i];

            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;

            var Admin = await _NarijeDBContext.Users
                            .Where(A => A.Id == Convert.ToInt32(Identity.FindFirst("Id").Value))
                            .AsNoTracking()
                            .FirstOrDefaultAsync();

            if (Admin is null)
                return null;

            //SMSHelper sms = new();
            //await sms.SendSMSMessage(_IConfiguration, _NarijeDBContext, (int)EnumSMSTarget.WalletEditStateCode, Admin, null, WalletPayment, code, 0, null, MakeJob: false);

            return new ApiOkResponse(_Message: "SUCCEED");

        }
        #endregion


    }
}


