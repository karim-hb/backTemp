using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Reserve;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;

namespace Narije.Api.Controllers.UserPanel.Reserve
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "user,admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("user/v{version:apiVersion}/[Controller]")]
    public class ReserveController : ControllerBase
    {
        private readonly IReserveRepository _IReserveRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public ReserveController(IReserveRepository iReserveRepository)
        {
            _IReserveRepository = iReserveRepository;
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
                return this.ServiceReturn(await _IReserveRepository.ExportAsync());
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
        [Route("Get")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return this.ServiceReturn(await _IReserveRepository.GetAsync(id: id));
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
                return this.ServiceReturn(await _IReserveRepository.GetAllAsync(page: page, limit: limit, byUser: true));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



    }
}

