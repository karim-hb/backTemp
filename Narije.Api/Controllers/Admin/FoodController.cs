using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Food;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;
using Narije.Infrastructure.Repositories;

namespace Narije.Api.Controllers.Admin.Food
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class FoodController : ControllerBase
    {
        private readonly IFoodRepository _IFoodRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public FoodController(IFoodRepository iFoodRepository)
        {
            _IFoodRepository = iFoodRepository;
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
                return this.ServiceReturn(await _IFoodRepository.ExportAsync());
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
                return this.ServiceReturn(await _IFoodRepository.GetAsync(id: id));
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
                return this.ServiceReturn(await _IFoodRepository.GetAllAsync(page: page, limit: limit));
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
                return this.ServiceReturn(await _IFoodRepository.DeleteAsync(id: id));
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
        public async Task<IActionResult> Insert([FromForm]FoodInsertRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IFoodRepository.InsertAsync(request: request));
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
        [Route("InsertFromExcel")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> InsertFromExcel( IFormFile file)
        {
            try
            {
                return this.ServiceReturn(await _IFoodRepository.ProcessFoodFileAsync(file: file));
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
        public async Task<IActionResult> Edit([FromForm]FoodEditRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IFoodRepository.EditAsync(request: request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// فعال/غیرفعال
        /// </summary>
        [HttpPut]
        [Route("EditActive")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> EditActive([FromForm] int id)
        {
            try
            {
                return this.ServiceReturn(await _IFoodRepository.EditActiveAsync(id));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}

