using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Wallet;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;

namespace Narije.Api.Controllers.Customer.Wallet
{
    /// <summary>
    /// زمانبندی
    /// </summary>
    [Authorize(Roles = "customer,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("customer/v{version:apiVersion}/[Controller]")]
    public class WalletController : ControllerBase
    {
        private readonly IWalletRepository _IWalletRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public WalletController(IWalletRepository iWalletRepository)
        {
            _IWalletRepository = iWalletRepository;
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
                return this.ServiceReturn(await _IWalletRepository.ExportWalletAsync());
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
                return this.ServiceReturn(await _IWalletRepository.GetAllAsync(page: page, limit: limit));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// دریافت مجموع
        /// </summary>
        [HttpGet]
        [Route("GetSum")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetSum(int? page, int? limit)
        {
            try
            {
                return this.ServiceReturn(await _IWalletRepository.GetSumAsync());
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}

