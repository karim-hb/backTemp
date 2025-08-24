using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Helpers;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.Public;

namespace Narije.Api.Controllers.Admin
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class ReciptsController : ControllerBase
    {
        private readonly IRecipts _IRecipts;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public ReciptsController(IRecipts iRecipts)
        {
            _IRecipts = iRecipts;
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
                return this.ServiceReturn(await _IRecipts.ExportAsync());
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
                return this.ServiceReturn(await _IRecipts.GetAllAsync(page: page, limit: limit));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// خروجی رسیپت اکسل بر اساس تاریخ و مشتری (اختیاری)
        /// </summary>
        [HttpGet]
        [Route("ExportRecipt")]
        [MapToApiVersion("2")]
        [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportRecipt(int? customerId, DateTime date)
        {
            try
            {
                var fileResult = await _IRecipts.ExportRecipt(customerId: customerId, date: date);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }

        /// <summary>
        /// خروجی رسیپت پی‌دی‌اف بر اساس تاریخ و مشتری (اختیاری)
        /// </summary>
        [HttpGet]
        [Route("ExportPdfRecipt")]
        [MapToApiVersion("2")]
        [Produces("application/pdf")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportPdfRecipt(int? customerId, DateTime date)
        {
            try
            {
                var fileResult = await _IRecipts.ExportPdfRecipt(customerId: customerId, date: date);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }

    }
}
