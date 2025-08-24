using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Narije.Api.Helpers
{
    /// <summary>
    /// OptionsMiddleware
    /// </summary>
    public class OptionsMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// OptionsMiddleware
        /// </summary>
        public OptionsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        public Task Invoke(HttpContext context)
        {
            return BeginInvoke(context);
        }

        /// <summary>
        /// BeginInvoke
        /// </summary>
        private Task BeginInvoke(HttpContext context)
        {
            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { (string)context.Request.Headers["Origin"] });
                context.Response.Headers.Add("Access-Control-Allow-Headers", new[] { "Content-Type, Authorization, Accept, Accept-Language, X-Authorization" });
                context.Response.Headers.Add("Access-Control-Allow-Methods", new[] { "GET, POST, PUT, DELETE, OPTIONS" });
                context.Response.Headers.Add("Access-Control-Allow-Credentials", new[] { "true" });
                context.Response.StatusCode = 200;
                return context.Response.WriteAsync("OK");
            }

            return _next.Invoke(context);
        }
    }

    /// <summary>
    /// OptionsMiddlewareExtensions
    /// </summary>
    public static class OptionsMiddlewareExtensions
    {
        /// <summary>
        /// UseOptions
        /// </summary>
        public static IApplicationBuilder UseOptions(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OptionsMiddleware>();
        }
    }
}
