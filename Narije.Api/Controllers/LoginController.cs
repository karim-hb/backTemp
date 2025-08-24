using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using mpNuget;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Narije.Api.Helpers;
using Narije.Core.DTOs.Enum;
using Narije.Core.DTOs.Login;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;

namespace Narije.Api.Controllers
{
    /// <summary>
    /// Login
    /// </summary>
    [Route("v1")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly NarijeDBContext _NarijeDBContext;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IConfiguration _IConfiguration;
        private TimeSpan Expire = new(hours: 24, minutes: 0, seconds: 0);

        /// <summary>
        /// متد سازنده
        /// </summary>
        public LoginController(NarijeDBContext NarijeDBContext, IHttpContextAccessor iHttpContextAccessor, IConfiguration iConfiguration)
        {
            _NarijeDBContext = NarijeDBContext;
            _IHttpContextAccessor = iHttpContextAccessor;
            _IConfiguration = iConfiguration;
        }

        private string GetUserRole(int role)
        {
            string res = "user";
            switch (role)
            {
                case (int)EnumRole.user:
                    res = "user";
                    break;
                case (int)EnumRole.supervisor:
                    res = "supervisor";
                    break;
                case (int)EnumRole.customer:
                    res = "customer";
                    break;
                case (int)EnumRole.superadmin:
                    res = "supervisor";
                    break;
            }

            return res;

        }

        /// <summary>
        /// خروج از سیستم
        /// </summary>
        [Route("logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var Identity = _IHttpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
                if (Identity is null)
                {
                    return StatusCode(StatusCodes.Status401Unauthorized, new ApiErrorResponse(_Message: "دسترسی شما منقضی گشته است"));
                }

                if (Identity.IsAuthenticated)
                {
                    /*
                    var User = await _NarijeDBContext.HrUser
                                                      .Where(A => A.Id == Convert.ToInt32(Identity.FindFirst("Id").Value))
                                                      .FirstOrDefaultAsync();
                    if (User != null)
                    {
                        User.RememberToken = null;
                        User.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                        _NarijeDBContext.HrUser.Update(User);
                        await _NarijeDBContext.SaveChangesAsync();
                    }
                    */
                }
                return Ok();
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// اعتبارسنجی کاربر
        /// </summary>
        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> UserLogin([FromBody] LoginRequest info)
        {
            try
            {

                var User = await _NarijeDBContext.Users
                                                  .Where(A => A.Mobile.Equals(info.mobile))
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync();
                if (User is null)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "کاربر یافت نشد"));
                }

                var PasswordVerified = BCrypt.Net.BCrypt.Verify(info.password, User.Password);
                if (PasswordVerified == false)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(_Message: "رمز عبور وارد شده اشتباه است"));
                }


                string role = GetUserRole(User.Role);

                if (role.Equals(info.panel) == false)
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(_Message: "مجوز دسترسی به این پنل را ندارید"));

                if (!User.Active)
                    return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(_Message: "مجوز دسترسی به این پنل را ندارید"));
                if (User.CustomerId != null && info.panel != "supervisor")
                {
                    var Customer = await _NarijeDBContext.Customers
                                                         .Where(c => c.Id == User.CustomerId)
                                                         .AsNoTracking()
                                                         .FirstOrDefaultAsync();
                    if (Customer is null)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(_Message: "شرکت مرتبط با حساب کاربری یافت نشد"));
                    }
                    if (!Customer.Active)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(_Message: "حساب شرکت مرتبط با شما غیرفعال است"));
                    }
                }
                var Claims = new List<Claim>
                        {
                            new Claim("Id", User.Id.ToString()),
                            new Claim(ClaimTypes.Role,role)
                        };

                var AccessToken = GenerateAccessToken(Claims);
                var RefreshToken = GenerateRefreshToken();

                User.LastLogin = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                _NarijeDBContext.Users.Update(User);
                await _NarijeDBContext.SaveChangesAsync();

                return Ok(new LoginResponse()
                {
                    UserId = User.Id,
                    Type = "bearer",
                    Role = role,
                    AccessToken = AccessToken,
                    ExpiresIn = (int)Expire.TotalSeconds
                });
            }
            catch (Exception Ex)
            {
                await Extension.LogError(Ex, _NarijeDBContext, _IHttpContextAccessor);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// ایجاد توکن
        /// </summary>
        private string GenerateAccessToken(IEnumerable<Claim> Claims)
        {
            var SecretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_IConfiguration["Jwt:Key"]));
            var SigninCredentials = new SigningCredentials(SecretKey, SecurityAlgorithms.HmacSha512Signature);
            var TokeOptions = new JwtSecurityToken(
            claims: Claims,
            expires: TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")).AddMinutes(Expire.TotalMinutes),
            signingCredentials: SigninCredentials);
            var TokenString = new JwtSecurityTokenHandler().WriteToken(TokeOptions);
            return TokenString;
        }

        /// <summary>
        /// ایجاد توکن
        /// </summary>
        private string GenerateRefreshToken()
        {
            var RandomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(RandomNumber);
                return Convert.ToBase64String(RandomNumber);
            }
        }
    }
}