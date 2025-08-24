using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Narije.Core.DTOs.Enum;
using Narije.Infrastructure.Contexts;

namespace Tikment.Api.Helpers
{
    /// <summary>
    /// CheckRoleActionFilter
    /// </summary>
    public class PermissionMiddleware : IAsyncActionFilter
    {
        /// <summary>
        /// CheckRoleActionFilter
        /// </summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var _WebHostEnvironment = context.HttpContext.RequestServices.GetService<IWebHostEnvironment>();
            var _NarijeDBContext = context.HttpContext.RequestServices.GetService<NarijeDBContext>();

            bool HasAllowAnonymous = context.ActionDescriptor.EndpointMetadata
                                                             .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));
            if (HasAllowAnonymous)
            {
                await next();
                return;
            }

            var Identity = context.HttpContext.User.Identity as ClaimsIdentity;
            if (Identity is null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var UrlMap = context.HttpContext.Request.Path.ToString().ToLower();
            if (Identity.FindFirst(ClaimTypes.Role) != null)
                if (Identity.FindFirst(ClaimTypes.Role).Value.Equals(EnumRole.supervisor.ToString()))  //SUPERADMIN
                {
                    await next();
                    return;
                }

            var Method = context.HttpContext.Request.Method;

            /*
            var AccessId = _NarijeDBContext.Users
                                          .Where(A => A.Id == Convert.ToInt32(Identity.FindFirst("Id").Value))
                                          .Select(A => A.AccessId)
                                          .FirstOrDefault();
            if (AccessId == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var PermissionNames = await _NarijeDBContext.AccessPermissions
                                           .Where(A => A.AccessId == AccessId)
                                           .Select(A => A.Permission.Value)
                                           .ToListAsync();
            */

            /*
            if (PermissionNames.Where(A => UrlMapExists.Contains(A)).FirstOrDefault() == null)
            {
                context.Result = new ForbidResult();
                return;
            }
            */

            await next();
        }
    }
}