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

namespace Narije.Api.Controllers.UserPanel.User
{
    /// <summary>
    /// کنترلر
    /// </summary>
    [Authorize(Roles = "user,admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("user/v{version:apiVersion}/[Controller]")]
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

