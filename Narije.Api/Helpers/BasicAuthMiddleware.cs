using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Narije.Api.Helpers
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public BasicAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (context.Request.Path.StartsWithSegments("/swagger"))
                {
                    string authHeader = context.Request.Headers["Authorization"];

                    if (authHeader != null && authHeader.StartsWith("Basic "))
                    {
                        var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                        var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                        var credentials = decodedCredentials.Split(':');

                        var username = credentials[0];
                        var password = credentials[1];

                        if (username == "karim" && password == "69")
                        {
                            await _next(context);
                            return;
                        }
                    }

                    context.Response.Headers["WWW-Authenticate"] = "Basic";
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {

            }

        }
    }

}
