using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.Customer;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;
using Narije.Infrastructure.Repositories;
using System.ComponentModel.Design;

namespace Narije.Api.Controllers.Admin.Customer
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _ICustomerRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public CustomerController(ICustomerRepository iCustomerRepository)
        {
            _ICustomerRepository = iCustomerRepository;
        }

        /// <summary>
        /// گزارش
        /// </summary>
        [HttpGet]
        [Route("Export")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Export(bool justBranches)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.ExportAsync(justBranches : justBranches));
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
        [Route("ExportBranch")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportBranch(int companyId)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.ExportBranch(companyId: companyId));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        /// خروجی اکسل    
        /// </summary>
        [HttpGet]
        [Route("ExportBranchServicesAsync")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ExportBranchServicesAsync(DateTime fromData, DateTime toData, int customerId = 0, bool showProductId = false, bool showFoodType = false, bool showFoodGroup = false,
            bool showVat = false, bool showArpa = false, bool showQty = false, bool isFood = false)
        {
            try
            {
                var fileResult = await _ICustomerRepository.ExportBranchServicesAsync(fromData: fromData, toData: toData, customerId: customerId, showProductId: showProductId, showFoodType: showFoodType,
                    showFoodGroup: showFoodGroup, showVat: showVat, showArpa: showArpa, showQty: showQty, isFood: isFood);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
            }
        }
        /// <summary>
        /// خروجی اکسل    
        /// </summary>
        [HttpGet]
        [Route("CustomerAccessoryExport")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> CustomerAccessoryExport(int companyId)
        {
            try
            {
                var fileResult = await _ICustomerRepository.CustomerAccessoryExport(companyId : companyId);
                return fileResult;
            }
            catch (Exception Ex)
            {
                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status400BadRequest, _Message: Ex.Message));
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
                return this.ServiceReturn(await _ICustomerRepository.GetAsync(id: id));
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
        public async Task<IActionResult> GetAll(int? page, int? limit, bool? onlyBranch)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.GetAllAsync(page: page, limit: limit, onlyBranch: onlyBranch));
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
        [Route("GetAllCustomerReport")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetAllCustomerReport(int? page, int? limit)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.GetAllCustomerReport(page: page, limit: limit));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        ///فهرست منو ها یک شرکت
        /// </summary>
        [HttpGet]
        [Route("GetAllCustomerMenuAsync")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetGetAllCustomerMenuAsyncAll(int customerId)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.GetAllCustomerMenuAsync(customerId: customerId));
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
        [Route("GetAllBranchesAsync")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetAllBranchesAsync(int? page, int? limit, int companyId)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.GetAllBranchesAsync(page: page, limit: limit, companyId: companyId));
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
        [Route("GetLastCode")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> GetLastCode(int? companyId)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.GetLastCodeAsync(companyId: companyId));
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
                return this.ServiceReturn(await _ICustomerRepository.DeleteAsync(id: id));
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
        public async Task<IActionResult> Insert([FromBody] CustomerInsertRequest request)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.InsertAsync(request: request));
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
        public async Task<IActionResult> Edit([FromBody] CustomerEditRequest request)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.EditAsync(request: request));
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
        [Route("EditActive")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> EditActive([FromForm] int id)
        {
            try
            {
                return this.ServiceReturn(await _ICustomerRepository.EditActiveAsync(id));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}

