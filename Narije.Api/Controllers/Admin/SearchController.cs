using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Search;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;

namespace Narije.Api.Controllers.Admin.Search
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchRepository _ISearchRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public SearchController(ISearchRepository iSearchRepository)
        {
            _ISearchRepository = iSearchRepository;
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
                return this.ServiceReturn(await _ISearchRepository.ExportAsync());
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
                return this.ServiceReturn(await _ISearchRepository.GetAsync(id: id));
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
                return this.ServiceReturn(await _ISearchRepository.GetAllAsync(page: page, limit: limit));
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
                return this.ServiceReturn(await _ISearchRepository.DeleteAsync(id: id));
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
        public async Task<IActionResult> Insert([FromForm]SearchInsertRequest request)
        {
            try
            {
                return this.ServiceReturn(await _ISearchRepository.InsertAsync(request: request));
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
        public async Task<IActionResult> Edit([FromForm]SearchEditRequest request)
        {
            try
            {
                return this.ServiceReturn(await _ISearchRepository.EditAsync(request: request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


    }
}

