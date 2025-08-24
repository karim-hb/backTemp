using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Narije.Core.DTOs.ViewModels.User;
using Narije.Core.DTOs.Public;
using Narije.Core.Interfaces;
using Narije.Api.Helpers;
using Narije.Core.DTOs.User;
using Narije.Infrastructure.Repositories;

namespace Narije.Api.Controllers.Admin.User
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("admin/v{version:apiVersion}/[Controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _IUserRepository;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public UserController(IUserRepository iUserRepository)
        {
            _IUserRepository = iUserRepository;
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
                return this.ServiceReturn(await _IUserRepository.ExportAsync());
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
                return this.ServiceReturn(await _IUserRepository.GetAsync(id: id));
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
                return this.ServiceReturn(await _IUserRepository.GetAllAsync(page: page, limit: limit));
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
                return this.ServiceReturn(await _IUserRepository.DeleteAsync(id: id));
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
        public async Task<IActionResult> Insert([FromForm]UserInsertRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IUserRepository.InsertAsync(request: request));
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
        public async Task<IActionResult> InsertFromExcel(IFormFile file)
        {
            try
            {
                return this.ServiceReturn(await _IUserRepository.ProcessUserFileAsync(file: file));
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
        public async Task<IActionResult> Edit([FromForm]UserEditRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IUserRepository.EditAsync(request: request));
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
                return this.ServiceReturn(await _IUserRepository.EditActiveAsync(id));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// دسترسی کاربر
        /// </summary>
        [HttpGet]
        [Route("UserPermissions")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> UserPermissions(int id)
        {
            try
            {
                return this.ServiceReturn(await _IUserRepository.UserPermissionsAsync());
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// تغییر رمز
        /// </summary>
        [HttpPut]
        [Route("ChangePassword")]
        [MapToApiVersion("2")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordRequest request)
        {
            try
            {
                return this.ServiceReturn(await _IUserRepository.ChangePasswordAsync(request));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

