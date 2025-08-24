using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.WalletPayment;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;
using Narije.Core.DTOs.Enum;
using Narije.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using BPService;

namespace Narije.Api.Controllers.Admin.WalletPayment
{
    /// <summary>
    /// زمانبندی
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class WalletPaymentController : ControllerBase
    {
        private readonly IWalletPaymentRepository _IWalletPaymentRepository;
        private readonly IConfiguration _IConfiguration;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public WalletPaymentController(IWalletPaymentRepository iWalletPaymentRepository, IConfiguration iConfiguration)
        {
            _IWalletPaymentRepository = iWalletPaymentRepository;
            _IConfiguration = iConfiguration;
        }

        /// <summary>
        /// گزارش
        /// </summary>
        [HttpGet]
        [Route("Export")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Export()
        {
            try
            {
                return this.ServiceReturn(await _IWalletPaymentRepository.ExportAsync());
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// دریافت یکی
        /// </summary>
        [HttpGet]
        [Route("Get")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return this.ServiceReturn(await _IWalletPaymentRepository.GetAsync(id: id));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// فهرست همه
        /// </summary>
        [HttpGet]
        [Route("GetAll")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetAll(int? page, int? limit)
        {
            try
            {
                return this.ServiceReturn(await _IWalletPaymentRepository.GetAllAsync(page: page, limit: limit));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// حذف
        /// </summary>
        [HttpDelete]
        [Route("Delete")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return this.ServiceReturn(await _IWalletPaymentRepository.DeleteAsync(id: id));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// درج
        /// </summary>
        [HttpPost]
        [Route("Insert")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Insert([FromForm] WalletPaymentInsertRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IWalletPaymentRepository.InsertAsync(request: request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ویرایش
        /// </summary>
        [HttpPut]
        [Route("Edit")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Edit([FromForm] WalletPaymentEditRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IWalletPaymentRepository.EditAsync(request: request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// تایید/رد
        /// </summary>
        [HttpPut]
        [Route("EditState")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> EditState([FromForm] int id, [FromForm] int state, [FromForm] string code)
        {
            try
            {
                return this.ServiceReturn(await _IWalletPaymentRepository.EditStateAsync(id, state));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ارسال کد صحت سنجی درخواست
        /// </summary>
        [HttpPut]
        [Route("EditStateCode")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> EditStateCode([FromForm] int id)
        {
            try
            {
                return this.ServiceReturn(await _IWalletPaymentRepository.EditStateCodeAsync(id));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        #region RecheckBankTransaction
        /// <summary>
        ///  بررسی مجدد تراکنش بانکی 
        /// </summary>
        /*
        [Route("RecheckBankTransaction")]
        [HttpPost]
        public async Task<IActionResult> RecheckBankTransaction([FromForm] int wpId)
        {
            try
            {
                var wp = await _IWalletPaymentRepository.RecheckBankTransactionAsync(wpId);

                if (wp.Message != null)
                    return StatusCode(StatusCodes.Status405MethodNotAllowed, new ApiErrorResponse(_Code: StatusCodes.Status405MethodNotAllowed, _Message: wp.Message));

                switch (wp.Gateway)
                {
                    case (int)EnumGateway.CityBank:
                        var ipgService = new IPGResetService(_IConfiguration);

                        var paymentResult = await ipgService.TranResult(int.Parse(_IConfiguration.GetSection("MerchantConfigurationId").Value), long.Parse(wp.token));
                        var result = new
                        {
                            pan = paymentResult.CardNumber,
                            result = paymentResult.ResMessage,
                            refNumber = paymentResult.PayGateTranID.ToString(),
                            amount = paymentResult.Amount,
                        };

                        return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: result));
                    case (int)EnumGateway.Mellat:
                        PaymentGatewayClient bpService = new PaymentGatewayClient();
                        var bpresult = await bpService.bpInquiryRequestAsync(Int64.Parse(_IConfiguration.GetSection("BPMTerminalId").Value),
                            _IConfiguration.GetSection("BPMUser").Value,
                            _IConfiguration.GetSection("BPMPassword").Value,
                            wpId, wpId, long.Parse(wp.token));

                        var result2 = new
                        {
                            pan = "",
                            result = bpresult.Body.@return,
                            refNumber = wp.token,
                            amount = 0//paymentResult.Amount,
                        };

                        return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: result2));
                    case (int)EnumGateway.Ayandeh:
                        AyandehRestService ayandeh = new();

                        var inquiry = await ayandeh.Inquiry(wp.token);
                        var result1 = new
                        {
                            pan = inquiry.mobile,
                            result = inquiry.result,
                            refNumber = inquiry.refNumber,
                            amount = inquiry.amount
                        };

                        return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: result1));
                }

                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: null));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        */
        #endregion
    }
}

