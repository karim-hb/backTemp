using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Helpers;
using Narije.Core.Interfaces;
using Narije.Infrastructure.Repositories;
using System.Threading.Tasks;
using System;

namespace Narije.Api.Controllers.Admin
{
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class MenuLogController : ControllerBase
    {
        private readonly IMenuLogRepository _IMenuLogRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public MenuLogController(IMenuLogRepository iMenuLogRepository)
        {
            _IMenuLogRepository = iMenuLogRepository;
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
                return this.ServiceReturn(await _IMenuLogRepository.ExportAsync());
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
        public async Task<IActionResult> GetAll(int? page, int? limit , int menuInfo)
        {
            try
            {
                return this.ServiceReturn(await _IMenuLogRepository.GetAllAsync(page: page, limit: limit, menuInfo: menuInfo));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
