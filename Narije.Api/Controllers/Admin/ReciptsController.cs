using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Helpers;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
        /// یرسی فعال بودن سفارش روز برای مشتری
        /// </summary>
        [HttpGet]
        [Route("ActiveReserve")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ActiveReserve(string customerIds, DateTime date)
        {
            try
            {
                return this.ServiceReturn(await _IRecipts.ActiveReserve(customerIds: customerIds, date: date));
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
        public async Task<IActionResult> ExportRecipt(string customerIds, DateTime date, bool all = false)
        {
            try
            {
                var fileResult = await _IRecipts.ExportRecipt(customerIds: customerIds, date: date , all: all);
                return fileResult;
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                while (ex != null)
                {
                    sb.AppendLine($"Message: {ex.Message}");
                    sb.AppendLine($"StackTrace: {ex.StackTrace}");
                    ex = ex.InnerException;
                }
                string fullError = sb.ToString();

                return BadRequest(new ApiErrorResponse(
                    _Code: StatusCodes.Status400BadRequest,
                    _Message: fullError
                ));
            }

        }

        /// <summary>
        /// خروجی رسیپت پی‌دی‌اف بر اساس تاریخ و مشتری (اختیاری)
        /// </summary>
        [HttpGet]
        [Route("ExportPdfRecipt")]
        [MapToApiVersion("2")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExportPdfRecipt(string customerIds, DateTime date, bool all = false)
        {
            try
            {
                var fileResult = await _IRecipts.ExportPdfRecipt(customerIds: customerIds, date: date, all: all);
                return fileResult;
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                while (ex != null)
                {
                    sb.AppendLine($"Message: {ex.Message}");
                    sb.AppendLine($"StackTrace: {ex.StackTrace}");
                    ex = ex.InnerException;
                }
                string fullError = sb.ToString();

                return BadRequest(new ApiErrorResponse(
                    _Code: StatusCodes.Status400BadRequest,
                    _Message: fullError
                ));
            }
        }

    }
}
