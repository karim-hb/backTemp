using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Narije.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Narije.Api.Helpers
{
    /// <summary>
    /// LogMiddleware
    /// </summary>
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// LogMiddleware
        /// </summary>
        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// InvokeAsync
        /// </summary>
        public async Task InvokeAsync(HttpContext context, NarijeDBContext _NarijeDBContext)
        {
            if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "DELETE")
            {
                var Route = context.Request.Path.ToString().ToLower();
                if (Route.StartsWith("/"))
                {
                    Route = Route.Remove(0, 1);
                }

                bool AcceptedRoute = false;
                List<string> AcceptedRegex = new()
                {
                    //"users",
                };


                foreach (var Str in AcceptedRegex)
                {
                    Regex Re = new(Str, RegexOptions.IgnoreCase);
                    if (Re.IsMatch(Route))
                    {
                        AcceptedRoute = true;
                        break;
                    }
                }

                if (AcceptedRoute == false)
                {
                    await _next(context);
                }

                var Identity = context.User.Identity as ClaimsIdentity;
                if (Identity is null)
                {
                    await _next(context);
                }

                if (Identity.IsAuthenticated)
                {
                    var UserId = Convert.ToInt32(Identity.FindFirst("Id").Value);
                    var Client = context.Connection.RemoteIpAddress.ToString();
                    string Parameters = null;
                    var Method = @"[""" + context.Request.Method + @"""]";

                    #region خواندن بادی درخواست و پاسخ
                    string Request = null, Response = null;
                    using (MemoryStream RequestBodyStream = new())
                    {
                        using MemoryStream ResponseBodyStream = new();
                        Stream OriginalRequestBody = context.Request.Body;
                        Stream OriginalResponseBody = context.Response.Body;
                        try
                        {
                            await context.Request.Body.CopyToAsync(RequestBodyStream);
                            RequestBodyStream.Seek(0, SeekOrigin.Begin);
                            Request = new StreamReader(RequestBodyStream).ReadToEnd();
                            RequestBodyStream.Seek(0, SeekOrigin.Begin);
                            context.Request.Body = RequestBodyStream;
                            context.Response.Body = ResponseBodyStream;

                            await _next(context);

                            ResponseBodyStream.Seek(0, SeekOrigin.Begin);
                            Response = new StreamReader(ResponseBodyStream).ReadToEnd();
                            ResponseBodyStream.Seek(0, SeekOrigin.Begin);
                            await ResponseBodyStream.CopyToAsync(OriginalResponseBody);
                        }
                        finally
                        {
                            context.Request.Body = OriginalRequestBody;
                            context.Response.Body = OriginalResponseBody;
                        }
                    }
                    #endregion
                    /*
                    var NewActivityLog = new ActivityLog()
                    {
                        UserId = UserId,
                        Client = Client,
                        Route = Route,
                        Parameters = Parameters ?? "[]",
                        Method = Method,
                        Request = Request,
                        Response = Response,
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
                    };

                    await _NarijeDBContext.ActivityLog.AddAsync(NewActivityLog);
                    await _NarijeDBContext.SaveChangesAsync();
                    */
                }
            }

            await _next(context);
        }
    }
}