using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using mpNuget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;
using Narije.Core.DTOs.Public;
using Narije.Api.Helpers;
using Narije.Core.DTOs.Admin;
using Narije.Core.DTOs.User;
using Narije.Core.DTOs.Enum;
using Microsoft.AspNetCore.Authentication;
using BPService;
using Hangfire.Logging;
using Hangfire;
using Narije.Api.Payment.BehPardakht;
using Narije.Api.Payment.AsanPardakht.models.sale;
using Narije.Api.Payment.AsanPardakht;
using Narije.Api.Payment.AsanPardakht.models.bill;
using Narije.Api.Payment.AsanPardakht.models.verify;
using Amazon.Auth.AccessControlPolicy;
using Narije.Api.Payment.ZarinPal;
using Narije.Api.Payment.ZarinPal.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Narije.Api.Controllers
{
    /// <summary>
    /// Home
    /// </summary>
    [Route("user")]
    [Authorize(Roles = "user,customer")]
    [ApiController]
    public class BankController : ControllerBase
    {
        private readonly NarijeDBContext _NarijeDBContext;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IConfiguration _IConfiguration;
        private readonly IHttpClientFactory _IHttpClientFactory;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        private static string BucketServiceURL = "https://tahlilmobile-gallery.storage.iran.liara.space/";

        /// <summary>
        /// متد سازنده
        /// </summary>
        public BankController(NarijeDBContext NarijeDBContext, IHttpContextAccessor iHttpContextAccessor, IConfiguration iConfiguration, IHttpClientFactory iHttpClientFactory, IWebHostEnvironment iWebHostEnvironment)
        {
            _NarijeDBContext = NarijeDBContext;
            _IHttpContextAccessor = iHttpContextAccessor;
            _IConfiguration = iConfiguration;
            _IHttpClientFactory = iHttpClientFactory;
            _IWebHostEnvironment = iWebHostEnvironment;

            BucketServiceURL = _IConfiguration.GetSection("baseURL").Value + "/public/downloadFileById/";
        }

        private async Task<User> CheckAccess()
        {
            //Check Access
            var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            if ((Identity is null) || (Identity.Claims.Count() == 0))
                return null;
            int id = Convert.ToInt32(Identity.FindFirst("Id").Value);
            var User = await _NarijeDBContext.Users
                                     .Where(A => A.Id == id)
                                     .Include(A => A.Customer)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();
            if (User is null)
                return null;

            return User;

        }

        private String GetResult(int gateway, string panel, int id, long price, string msg, int state = 400, string refNumber = "")
        {
            refNumber ??= " ";
            panel ??= "user";
            if(panel.Equals("panel"))
                return $"{_IConfiguration.GetSection("PanelRedirectUrl").Value}?gateway={gateway}&invoiceId={id}&state={state}&price={price}&refNumber={refNumber}&message={System.Net.WebUtility.UrlEncode(msg)}";
            else
                return $"{_IConfiguration.GetSection("FrontRedirectUrl").Value}?gateway={gateway}&invoiceId={id}&state={state}&price={price}&refNumber={refNumber}&message={System.Net.WebUtility.UrlEncode(msg)}";
        }

        #region ZReturn-----------------------------------------------------
        /// <summary>
        /// </summary>
        [AllowAnonymous]
        [Route("ZReturn/{id}/{panel}")]
        [HttpGet]
        public async Task<IActionResult> ZReturn([FromRoute] int id, [FromRoute] string panel, [FromQuery]string Authority, [FromQuery]string Status)
        {

            DateTime LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

            WalletPayment wp = await _NarijeDBContext.WalletPayments.Where(A => A.Id == id).FirstOrDefaultAsync();
            if (wp is null)
                return Redirect(GetResult((int)EnumGateway.ZarinPal, panel, 0, 0, "شماره پرداخت یافت نشد"));

            PaymentResultVm paymentResult = new();
            String SaleReferenceId = "";

            try
            {
                switch (wp.Gateway)
                {
                    case (int)EnumGateway.ZarinPal:

                        paymentResult.Amount = wp.Value * 10;

                        String MerchantID = "92cf6409-648a-4f01-904b-85a32c6122e9";

                        PaymentVerification verify = new PaymentVerification()
                        {
                            amount = paymentResult.Amount.ToString(),
                            authority = Authority,
                            merchant_id = MerchantID
                        };

                        ZarinPal zarin = new ZarinPal();
                        var result = await zarin.PaymentVerification(verify);
                        if (result is null)
                            return Redirect(GetResult((int)EnumGateway.ZarinPal, panel, wp.Id, wp.Value, "پاسخی دریافت نشد"));
                        else
                        {
                            paymentResult.PayGateTranID = (long)result.ref_id;
                            SaleReferenceId = result.ref_id.ToString();
                            paymentResult.CardNumber = result.card_pan;
                            paymentResult.ResCode = result.code;

                        }
                        break;
                }

                if ((paymentResult.ResCode != 100) && (paymentResult.ResCode != 101))
                    return Redirect(GetResult((int)EnumGateway.ZarinPal, panel, wp.Id, wp.Value, "تراکنش موفقیت آمیز نبوده است", 400, SaleReferenceId));

                if (wp.Value != paymentResult.Amount / 10)
                    return Redirect(GetResult((int)EnumGateway.ZarinPal, panel, wp.Id, wp.Value, "رقم تراکنش تطابق ندارد", 400, SaleReferenceId));
            }
            catch (Exception Ex)
            {
                return Redirect(GetResult((int)EnumGateway.ZarinPal, panel, wp.Id, wp.Value, "خطا در ثبت تراکنش مالی" + Ex.Message, 400, SaleReferenceId));
            }

            using var Transaction = _NarijeDBContext.Database.BeginTransaction();
            try
            {

                var Wallet = await _NarijeDBContext.Wallets
                                    .Where(A => A.UserId == wp.UserId)
                                    .OrderByDescending(A => A.Id)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
                long prevalue = 0;
                if (Wallet != null)
                    prevalue = Wallet.RemValue;

                Wallet wallet = new Wallet()
                {
                    DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                    Op = (int)EnumWalletOp.Credit,
                    Value = wp.Value,
                    UserId = wp.UserId,
                    PreValue = prevalue,
                    RemValue = prevalue + wp.Value
                };
                wallet.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(wallet);
                await _NarijeDBContext.Wallets.AddAsync(wallet);
                await _NarijeDBContext.SaveChangesAsync();

                wp.RefNumber = SaleReferenceId;
                wp.Result = paymentResult.ResMessage;
                wp.Status = 1;
                wp.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                wp.Pan = paymentResult.CardNumber;
                wp.WalletId = wallet.Id;

                _NarijeDBContext.WalletPayments.Update(wp);

                await _NarijeDBContext.SaveChangesAsync();

                await Transaction.CommitAsync();
                return Redirect(GetResult((int)EnumGateway.ZarinPal, panel, wp.Id, wp.Value, "پرداخت انجام شد", 200, SaleReferenceId));

            }
            catch (Exception Ex)
            {
                if (Transaction != null)
                    await Transaction.RollbackAsync();
                //await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return Redirect(GetResult((int)EnumGateway.ZarinPal, panel, wp.Id, wp.Value, "خطا در ثبت تراکنش مالی" + Ex.Message, 400, SaleReferenceId));
            }


        }
        #endregion

        #region BPMReturn-----------------------------------------------------
        /// <summary>
        /// </summary>
        [AllowAnonymous]
        [Route("BPMReturn/{panel}")]
        [HttpPost]
        public async Task<IActionResult> BPMReturn([FromRoute] string panel, [FromForm] string RefId, [FromForm] string ResCode, [FromForm] string SaleOrderId,
                                                    [FromForm] string SaleReferenceId, [FromForm] string CardHolderPan)
        {
            DateTime LocalDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));

            WalletPayment wp = await _NarijeDBContext.WalletPayments.Where(A => A.Id.ToString().Equals(SaleOrderId)).FirstOrDefaultAsync();
            if (wp is null)
                return Redirect(GetResult((int)EnumGateway.Mellat, panel, 0, 0, "شماره پرداخت یافت نشد"));
            
            PaymentResultVm paymentResult = new();


            try
            {
                switch (wp.Gateway)
                {
                     case (int)EnumGateway.Mellat:
                        paymentResult.Amount = wp.Value * 10;
                        paymentResult.PayGateTranID = SaleReferenceId == null ? null : long.Parse(SaleReferenceId);
                        paymentResult.CardNumber = CardHolderPan;

                        if (ResCode.Equals("0"))
                        {
                            PaymentGatewayClient bpService = new PaymentGatewayClient();
                            var bpresult = await bpService.bpVerifyRequestAsync(Int64.Parse(_IConfiguration.GetSection("BPMTerminalId").Value),
                                _IConfiguration.GetSection("BPMUser").Value,
                                _IConfiguration.GetSection("BPMPassword").Value,
                                wp.Id, wp.Id, paymentResult.PayGateTranID.Value);

                            //var resultArray = bpresult.Body.@return.Split(',');
                            if (bpresult.Body.@return.Length == 0)
                            {
                                paymentResult.ResMessage = "پاسخی دریافت نشد";
                                paymentResult.ResCode = 800;
                            }
                            else
                            {
                                paymentResult.ResCode = int.Parse(bpresult.Body.@return);
                                paymentResult.ResMessage = BPUtils.ErrorMessage(paymentResult.ResCode);

                                if (paymentResult.ResCode == 43)
                                    return Redirect(GetResult((int)EnumGateway.Mellat, panel, wp.Id, wp.Value, "پرداخت انجام شد", 200, wp.RefNumber));
                            }
                        }
                        else
                        {
                            paymentResult.ResCode = Int32.Parse(ResCode);
                            paymentResult.ResMessage = BPUtils.ErrorMessage(paymentResult.ResCode);
                        }
                        break;

                }

                ////در صورتی که فیلد زیر مقدار صفر داشته باشد پرداخت با موفقیت انجام شده است
                if (paymentResult.ResCode != 0)
                    return Redirect(GetResult((int)EnumGateway.Mellat, panel, wp.Id, wp.Value, "تراکنش موفقیت آمیز نبوده است", 400, SaleReferenceId));

                if (wp.Value != paymentResult.Amount / 10)
                    return Redirect(GetResult((int)EnumGateway.Mellat, panel, wp.Id, wp.Value, "رقم تراکنش تطابق ندارد", 400, SaleReferenceId));
            }
            catch (Exception Ex)
            {
                return Redirect(GetResult((int)EnumGateway.Mellat, panel, wp.Id, wp.Value, "خطا در ثبت تراکنش مالی" + Ex.Message, 400, SaleReferenceId));
            }

            using var Transaction = _NarijeDBContext.Database.BeginTransaction();
            try
            {

                var Wallet = await _NarijeDBContext.Wallets
                                    .Where(A => A.UserId == wp.UserId)
                                    .OrderByDescending(A => A.Id)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
                long prevalue = 0;
                if (Wallet != null)
                    prevalue = Wallet.RemValue;

                Wallet wallet = new Wallet()
                {
                    DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                    Op = (int)EnumWalletOp.Credit,
                    Value = wp.Value,
                    UserId = wp.UserId,
                    PreValue = prevalue,
                    RemValue = prevalue + wp.Value
                };
                wallet.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(wallet);
                await _NarijeDBContext.Wallets.AddAsync(wallet);
                await _NarijeDBContext.SaveChangesAsync();

                wp.RefNumber = SaleReferenceId;
                wp.Result = paymentResult.ResMessage;
                wp.Status = 1;
                wp.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                wp.Pan = paymentResult.CardNumber;
                wp.WalletId = wallet.Id;

                _NarijeDBContext.WalletPayments.Update(wp);

                await _NarijeDBContext.SaveChangesAsync();

                await Transaction.CommitAsync();
                return Redirect(GetResult((int)EnumGateway.Mellat, panel, wp.Id, wp.Value, "پرداخت انجام شد", 200, SaleReferenceId));

            }
            catch (Exception Ex)
            {
                if (Transaction != null)
                    await Transaction.RollbackAsync();
                //await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return Redirect(GetResult((int)EnumGateway.Mellat, panel, wp.Id, wp.Value, "خطا در ثبت تراکنش مالی" + Ex.Message, 400, SaleReferenceId));
            }


        }
        #endregion

        /// <summary>
        /// شارژ کیف پول
        /// به تومان ارسال گردد
        /// </summary>
        [Route("chargeWallet")]
        [HttpPost]
        public async Task<IActionResult> ChargeWallet([FromForm] int Payment, [FromForm] int? Gateway)
        {
            try
            {
                var User = await CheckAccess();
                if (User is null)
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

                if (Payment > 200000000)
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "رقم شارژ بیش از سقف تراکنش بانکی است"));

                ulong Price = (ulong)Payment * 10;
                WalletPayment wp = new WalletPayment()
                {
                    Status = 0,
                    UserId = User.Id,
                    DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                    Op = (int)EnumWalletOp.Credit,
                    Value = Payment,
                    Gateway = Gateway.Value
                };

                await _NarijeDBContext.WalletPayments.AddAsync(wp);
                await _NarijeDBContext.SaveChangesAsync();

                int InvoiceID = wp.Id;
                String panel = (User.Role == (int)EnumRole.customer) ? "panel" : "user";

                switch (Gateway)
                {
                    case (int)EnumGateway.Bank:
                        var paymentToken = new SaleCommand(int.Parse(_IConfiguration.GetSection("MerchantConfigurationId").Value),
                                                         Convert.ToInt32(ServiceTypeEnum.Sale),
                                                         InvoiceID,
                                                         Price,
                                                         $"{_IConfiguration.GetSection("IPGRedirectUrl").Value}?invoiceID={InvoiceID}",
                                                         string.Empty
                                                        );

                        var ipgService = new IPGResetService(_IConfiguration);
                        var result = ipgService.Token<SaleCommand, SaleTokenVm>(paymentToken).Result;

                        if (result.ResCode == 0)
                        {
                            var zdata = new
                            {
                                gateway = "https://asan.shaparak.ir",
                                nvc = new
                                {
                                    RefId = result.RefId,
                                    mobileap = User.Mobile
                                }
                            };
                            return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: zdata));
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: $"خطا در اتصال به درگاه {result.ResCode}"));
                        }
                        break;
                    case (int)EnumGateway.ZarinPal:
                        String MerchantID = "92cf6409-648a-4f01-904b-85a32c6122e9";
                        String CallbackURL = $"{_IConfiguration.GetSection("ZPRedirectUrl").Value}/{wp.Id}/{panel}";

                        ZarinPal zarin = new ZarinPal();
                        string authority = await zarin.PaymentRequest(new PaymentRequest(merchant_id: MerchantID, amount: Price.ToString(), callback_url: CallbackURL, description: "شارژ کیف پول"));

                        var data = new
                        {
                            gateway = zarin.CreateUrl(authority),
                            nvc = new
                            {
                                RefId = "",
                                mobileap = User.Mobile
                            }
                        };
                        return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data));
                        break;
                    case (int)EnumGateway.Mellat:
                        PaymentGatewayClient bpService = new PaymentGatewayClient();
                        var bptoken = await bpService.bpPayRequestAsync(Int64.Parse(_IConfiguration.GetSection("BPMTerminalId").Value),
                            _IConfiguration.GetSection("BPMUser").Value,
                            _IConfiguration.GetSection("BPMPassword").Value,
                            InvoiceID,
                            (long)Price,
                            DateTime.Now.ToString("yyyyMMdd"),
                            DateTime.Now.ToString("hhmmss"),
                            string.Empty,
                            $"{_IConfiguration.GetSection("BPMRedirectUrl").Value}/{panel}?invoiceID={InvoiceID}",
                            _IConfiguration.GetSection("BPMMerchantId").Value,
                            User.Mobile, string.Empty, string.Empty, string.Empty, string.Empty);

                        var resultArray = bptoken.Body.@return.Split(',');
                        if (resultArray.Length == 0)
                        {
                            return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "پاسخی از بانک دریافت نگردید"));
                        }
                        if (resultArray[0] != "0")
                        {
                            return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "خطای هدایت به بانک " + resultArray[0]));
                        }
                        var data1 = new
                        {
                            gateway = "https://bpm.shaparak.ir/pgwchannel/startpay.mellat",
                            nvc = new
                            {
                                RefId = resultArray[1],
                                mobileap = User.Mobile
                            }
                        };
                        return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: data1));
                    default:
                        return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "درگاه را به درستی انتخاب نمایید"));
                }


            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// ثبت پاسخ تراکنش بانکی
        /// source:
        ///   0:  wallet
        /// </summary>
        [Route("apReturn")]
        [HttpPost]
        public async Task<IActionResult> APReturn([FromForm] int? source,
                                                  [FromForm] int? invoiceId,
                                                [FromForm] int? ResCode,
                                                [FromForm] long? SaleReferenceId,
                                                [FromForm] string CardHolderPan)
        {
            Wallet UserWallet = null;
            IPGResetService ipgService = null;

            var User = await CheckAccess();
            if (User is null)
                return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));

            WalletPayment wp = null;

            source = 0;

            if (invoiceId is null)
                return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "شماره سفارش تعیین نشده است"));


            PaymentResultVm paymentResult = new();

            try
            {
                switch (source)
                {
                    //شارژ کیف پول
                    case 0:
                        wp = await _NarijeDBContext.WalletPayments.Where(A => A.Id == invoiceId).FirstOrDefaultAsync();
                        if (wp is null)
                            return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "شماره سفارش وجود ندارد"));

                        switch (wp.Gateway)
                        {
                            case (int)EnumGateway.Bank:
                                ipgService = new IPGResetService(_IConfiguration);
                                paymentResult = await ipgService
                                    .TranResult(int.Parse(_IConfiguration.GetSection("MerchantConfigurationId").Value),
                                        invoiceId.Value);
                                break;
                            case (int)EnumGateway.Mellat:
                                paymentResult.Amount = wp.Value * 10;
                                paymentResult.PayGateTranID = SaleReferenceId;
                                paymentResult.CardNumber = CardHolderPan;

                                if (ResCode == 0)
                                {
                                    PaymentGatewayClient bpService = new PaymentGatewayClient();
                                    var bpresult = await bpService.bpVerifyRequestAsync(Int64.Parse(_IConfiguration.GetSection("BPMTerminalId").Value),
                                        _IConfiguration.GetSection("BPMUser").Value,
                                        _IConfiguration.GetSection("BPMPassword").Value,
                                        wp.Id, wp.Id, SaleReferenceId.Value);

                                    //var resultArray = bpresult.Body.@return.Split(',');
                                    if (bpresult.Body.@return.Length == 0)
                                    {
                                        paymentResult.ResMessage = "پاسخی دریافت نشد";
                                        paymentResult.ResCode = 800;
                                    }
                                    else
                                    {
                                        paymentResult.ResCode = int.Parse(bpresult.Body.@return);
                                        paymentResult.ResMessage = BPUtils.ErrorMessage(paymentResult.ResCode);

                                        if (paymentResult.ResCode == 43)
                                            return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: new { invoiceId = invoiceId, referenceNumber = SaleReferenceId, gateway = "درگاه پرداخت اینترنتی" }));
                                    }
                                }
                                else
                                {
                                    paymentResult.ResCode = ResCode.Value;
                                    paymentResult.ResMessage = BPUtils.ErrorMessage(paymentResult.ResCode);
                                }
                                break;

                        }

                        ////در صورتی که فیلد زیر مقدار صفر داشته باشد پرداخت با موفقیت انجام شده است
                        if (paymentResult.ResCode != 0)
                            return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "تراکنش موفقیت آمیز نبوده است"));

                        if (wp.Value != paymentResult.Amount / 10)
                            return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: "رقم تراکنش تطابق ندارد"));

                        break;
                }

            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }


            if (ipgService != null)
            {
                var verifyCommand = new VerifyCommand()
                {
                    merchantConfigurationId = int.Parse(_IConfiguration.GetSection("MerchantConfigurationId").Value),
                    payGateTranId = (ulong)paymentResult.PayGateTranID.Value
                };
                var verifyRes = ipgService.VerifyTrx(verifyCommand).Result;
                if (verifyRes.ResCode != 0)
                {
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Message: verifyRes.ResMessage));
                }
            }

            using var Transaction = _NarijeDBContext.Database.BeginTransaction();
            try
            {
                switch (source)
                {
                    case 0:
                        var Wallet = await _NarijeDBContext.Wallets
                                            .Where(A => A.UserId == User.Id)
                                            .OrderByDescending(A => A.Id)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();
                        long prevalue = 0;
                        if (Wallet != null)
                            prevalue = Wallet.RemValue;

                        Wallet wallet = new Wallet()
                        {
                            DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                            Op = (int)EnumWalletOp.Credit,
                            Value = wp.Value,
                            UserId = User.Id,
                            PreValue = prevalue,
                            RemValue = prevalue + wp.Value
                        };
                        wallet.Opkey = Narije.Infrastructure.Helpers.SecurityHelper.WalletGenerateKey(wallet);
                        await _NarijeDBContext.Wallets.AddAsync(wallet);
                        await _NarijeDBContext.SaveChangesAsync();

                        wp.RefNumber = paymentResult.PayGateTranID.ToString();
                        wp.Result = paymentResult.ResMessage;
                        wp.Status = 1;
                        wp.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                        wp.Pan = paymentResult.CardNumber;
                        wp.WalletId = wallet.Id;

                        _NarijeDBContext.WalletPayments.Update(wp);
                        break;
                }
                await _NarijeDBContext.SaveChangesAsync();

                await Transaction.CommitAsync();

            }
            catch (Exception Ex)
            {
                if (Transaction != null)
                    await Transaction.RollbackAsync();
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            string refrenceNumber = "";
            refrenceNumber = wp.RefNumber;

            return Ok(new ApiOkResponse(_Message: "SUCCEED", _Data: new { invoiceId = invoiceId, referenceNumber = refrenceNumber, gateway = "درگاه پرداخت اینترنتی" }));

        }


        /// <summary>
        /// ثبت پاسخ تراکنش بانکی
        /// source:
        ///   0:  wallet
        /// </summary>
        [AllowAnonymous()]
        [Route("apReturnTest")]
        [HttpPost]
        public async Task<IActionResult> APReturnTest([FromForm] int? invoiceId,
                                                [FromForm] int? ResCode,
                                                [FromForm] long? SaleReferenceId,
                                                [FromForm] string CardHolderPan)
        {
            return Redirect("https://www.time.ir");
        }


    }

}