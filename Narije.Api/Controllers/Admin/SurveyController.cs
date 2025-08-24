using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Survey;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;

namespace Narije.Api.Controllers.Admin.Survey
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly ISurveyRepository _ISurveyRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public SurveyController(ISurveyRepository iSurveyRepository)
        {
            _ISurveyRepository = iSurveyRepository;
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
                return this.ServiceReturn(await _ISurveyRepository.ExportAsync());
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
                return this.ServiceReturn(await _ISurveyRepository.GetAsync(id: id));
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
                return this.ServiceReturn(await _ISurveyRepository.GetAllAsync(page: page, limit: limit));
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
                return this.ServiceReturn(await _ISurveyRepository.DeleteAsync(id: id));
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
        public async Task<IActionResult> Insert([FromForm]SurveyInsertRequest request)
        {
            try
            {
                return this.ServiceReturn(await _ISurveyRepository.InsertAsync(request: request));
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
        public async Task<IActionResult> Edit([FromForm]SurveyEditRequest request)
        {
            try
            {
                return this.ServiceReturn(await _ISurveyRepository.EditAsync(request: request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// فهرست مثبت
        /// </summary>
        [HttpGet]
        [Route("GetPositive")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetPositive(int? page, int? limit)
        {
            try
            {
                return this.ServiceReturn(await _ISurveyRepository.GetPositiveAsync(page: page, limit: limit));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// فهرست منفی
        /// </summary>
        [HttpGet]
        [Route("GetNegative")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetNegative(int? page, int? limit)
        {
            try
            {
                return this.ServiceReturn(await _ISurveyRepository.GetNegativeAsync(page: page, limit: limit));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// پاراتل
        /// </summary>
        [HttpGet]
        [Route("Paratel")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Paratel()
        {
            try
            {
                return this.ServiceReturn(await _ISurveyRepository.ParatelAsync());
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// پاراتل
        /// </summary>
        [HttpGet]
        [Route("ExportParatel")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportParatel()
        {
            try
            {
                return this.ServiceReturn(await _ISurveyRepository.ExportParatelAsync());
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// فهرست مثبت
        /// </summary>
        [HttpGet]
        [Route("ExportPositive")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportPositive()
        {
            try
            {
                return this.ServiceReturn(await _ISurveyRepository.ExportPositiveAsync());
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// فهرست منفی
        /// </summary>
        [HttpGet]
        [Route("ExportNegative")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportNegative()
        {
            try
            {
                return this.ServiceReturn(await _ISurveyRepository.ExportNegativeAsync());
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

