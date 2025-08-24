using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Helpers;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Repositories;
using System.Threading.Tasks;
using System;

namespace Narije.Api.Controllers.Customer
{    /// <summary>
     /// کنترلر
     /// </summary>
    [Authorize(Roles = "customer,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("customer/v{version:apiVersion}/[Controller]")]
    public class WidgetController  : ControllerBase
    {
          private readonly ICustomerWidget _ICustomerWidget;
        /// <summary>
        /// متد سازنده
        /// </summary>
        public WidgetController(ICustomerWidget iCustomerWidget)
        {
            _ICustomerWidget = iCustomerWidget;
        }
        /// <summary>
        /// گزارش
        /// </summary>
        [HttpGet]
        [Route("GetSummary")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                return this.ServiceReturn(await _ICustomerWidget.GetSummary());
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        /// گزارش
        /// </summary>
        [HttpGet]
        [Route("GetReserves")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetReserves(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerWidget.GetReserves(fromDate: fromDate , toDate: toDate));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
