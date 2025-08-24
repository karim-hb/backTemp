using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Helpers;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Repositories;
using System.Threading.Tasks;
using System;
using Narije.Core.DTOs.ViewModels.Customer;
using Narije.Core.DTOs.ViewModels.CustomerMenuInfo;

namespace Narije.Api.Controllers.Admin
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class CustomerMenuInfoController : ControllerBase
    {
        private readonly ICustomerMenuInfo _ICustomerMenuInfo;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public CustomerMenuInfoController(ICustomerMenuInfo iCustomerMenuInfo)
        {
            _ICustomerMenuInfo = iCustomerMenuInfo;
        }


        /// <summary>
        /// فهرست همه
        /// </summary>
        [HttpGet]
        [Route("GetAll")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetAll(int customerId)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerMenuInfo.GetAllAsync(customerId: customerId));
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
        public async Task<IActionResult> Insert([FromBody] CustomerMenuInfoRequest[] request)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerMenuInfo.EditInsertAsync(request: request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
