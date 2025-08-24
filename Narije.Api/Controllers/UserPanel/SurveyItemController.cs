using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.SurveyItem;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;

namespace Narije.Api.Controllers.UserPanel.SurveyItem
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "user,admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("user/v{version:apiVersion}/[Controller]")]
    public class SurveyItemController : ControllerBase
    {
        private readonly ISurveyItemRepository _ISurveyItemRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public SurveyItemController(ISurveyItemRepository iSurveyItemRepository)
        {
            _ISurveyItemRepository = iSurveyItemRepository;
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
                return this.ServiceReturn(await _ISurveyItemRepository.GetAsync(id: id));
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
                return this.ServiceReturn(await _ISurveyItemRepository.GetAllAsync(page: page, limit: limit));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}

