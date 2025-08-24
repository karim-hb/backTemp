using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Narije.Infrastructure.Helpers
{
    public class TokenHelper
    {
        public static int GetUserId(IHttpContextAccessor httpContextAccessor)
        {
            var identity = httpContextAccessor?.HttpContext?.User?.Identity as ClaimsIdentity;
            int id = Convert.ToInt32(identity.FindFirst("Id")?.Value);
            return id;
        }
    }
}
