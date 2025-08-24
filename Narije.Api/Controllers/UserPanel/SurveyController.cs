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

namespace Narije.Api.Controllers.UserPanel.Survey
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "user,admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("user/v{version:apiVersion}/[Controller]")]
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
                return this.ServiceReturn(await _ISurveyRepository.GetAllAsync(page: page, limit: limit, true));
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
        public async Task<IActionResult> Insert([FromForm] SurveyInsertRequest request)
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

    }
}

