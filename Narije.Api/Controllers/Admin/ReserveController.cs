using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Reserve;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;
using Narije.Core.DTOs.Public;
using System.Collections.Generic;

namespace Narije.Api.Controllers.Admin.Reserve
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
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
        /// دریافت یکی
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
        public async Task<IActionResult> GetAll(int? page, int? limit, bool justPredict = false)
        {
            try
            {
                return this.ServiceReturn(await _IReserveRepository.GetAllAsync(page: page, limit: limit,false, justPredict: justPredict));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        /// خروجی اکسل بر اساس شرکت ها
        /// </summary>
        [HttpGet]
        [Route("ExportBranchServicesAsync")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportBranchServicesAsync(DateTime fromData , DateTime toData, bool predict)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportBranchServicesAsync(fromData: fromData, toData: toData, predict: predict);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }

        /// <summary>
        /// خروجی اکسل بر اساس شرکت ها
        /// </summary>
        [HttpGet]
        [Route("ExportFoodBaseOnDayAsync")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportFoodBaseOnDayAsync(DateTime fromData, DateTime toData, bool isFood)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportFoodBaseOnDayAsync(fromData: fromData, toData: toData , isFood: isFood);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }
        /// <summary>
        /// خروجی اکسل بر اساس شرکت ها
        /// </summary>
        [HttpGet]
        [Route("ExportDailyBaseOnBranchesAndFoodAsync")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportDailyBaseOnBranchesAndFoodAsync(DateTime fromData, DateTime toData, bool isFood)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportDailyBaseOnBranchesAndFoodAsync(fromData: fromData, toData: toData, isFood: isFood);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }
        /// <summary>
        ///ExportReserveBaseOnTheFood
        /// </summary>
        [HttpGet]
        [Route("ExportReserveBaseOnTheFood")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportReserveBaseOnTheFood(DateTime fromData, DateTime toData, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportReserveBaseOnTheFood(fromData: fromData, toData: toData,  foodGroupIds : foodGroupIds , showAccessory: showAccessory, justPredict: justPredict);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }
        /// <summary>
        ///ExportReserveBaseOnTheBranches
        /// </summary>
        [HttpGet]
        [Route("ExportReserveBaseOnTheBranches")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportReserveBaseOnTheBranches(DateTime fromData, DateTime toData, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportReserveBaseOnTheBranches(fromData: fromData, toData: toData, foodGroupIds: foodGroupIds, showAccessory: showAccessory, justPredict: justPredict);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }
        /// <summary>
        ///ExportReserveBaseOnTheCustomers
        /// </summary>
        [HttpGet]
        [Route("ExportReserveBaseOnTheCustomers")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportReserveBaseOnTheCustomers(DateTime fromData, DateTime toData,  string foodGroupIds = null, bool showAccessory = false, bool justPredict = false, bool isPdf = false)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportReserveBaseOnTheCustomers(fromData: fromData, toData: toData,  foodGroupIds: foodGroupIds, showAccessory: showAccessory, justPredict: justPredict, isPdf: isPdf);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }

        /// <summary>
        /// ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheFood
        /// </summary>
        [HttpGet]
        [Route("ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheFood")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheFood(DateTime fromData, DateTime toData)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheFood(fromData: fromData, toData: toData);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }

        /// <summary>
        /// ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheCustomers
        /// </summary>
        [HttpGet]
        [Route("ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheCustomers")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheCustomers(DateTime fromData, DateTime toData)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheCustomers(fromData: fromData, toData: toData);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }

        /// <summary>
        /// ExportDifferenceBetweenPredictAndNormalReserveBaseOnTheBranches
        /// </summary>
        [HttpGet]
        [Route("ExportDifferenceBetweenPredictAndNormalReserveBaseOnTheBranches")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportDifferenceBetweenPredictAndNormalReserveBaseOnTheBranches(DateTime fromData, DateTime toData)
        {
            try
            {
                var fileResult = await _IReserveRepository.ExportDifferenceBetweenPredictAndNormalReserveBaseOnTheBranches(fromData: fromData, toData: toData);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }

        /// <summary>
        /// GetAllByParams
        /// </summary>
        [HttpGet]
        [Route("GetAllByParams")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetAllByParams(int? page, int? limit, int paramsId, string paramsName, string headerName)
        {
            try
            {
                return this.ServiceReturn(await _IReserveRepository.GetAllByParamsAsync(page: page, limit: limit, paramsId: paramsId, paramsName: paramsName, headerName: headerName));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// حذف
        /// </summary>
        [HttpPost]
        [Route("Delete")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return this.ServiceReturn(await _IReserveRepository.DeleteAsync(id: id));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// افزودن
        /// </summary>
        [HttpPost]
        [Route("Insert")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Insert(ReserveInsertRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IReserveRepository.InsertAsync(request: request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ویرایش
        /// </summary>
        [HttpPost]
        [Route("Edit")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Edit(ReserveEditRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IReserveRepository.EditAsync(request: request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

