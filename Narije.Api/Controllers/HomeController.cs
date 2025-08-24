using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Narije.Api.Helpers;
using Narije.Core.DTOs.Home;
using Narije.Core.DTOs.Public;
using Narije.Infrastructure.Contexts;

namespace Narije.Api.Controllers
{
    /// <summary>
    /// Home
    /// </summary>
    [Route("home")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly NarijeDBContext _NarijeDBContext;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IConfiguration _IConfiguration;
        private readonly IHttpClientFactory _IHttpClientFactory;
        private readonly IWebHostEnvironment _IWebHostEnvironment;

        /// <summary>
        /// متد سازنده
        /// </summary>
        public HomeController(NarijeDBContext NarijeDBContext, IHttpContextAccessor iHttpContextAccessor, IConfiguration iConfiguration, IHttpClientFactory iHttpClientFactory, IWebHostEnvironment iWebHostEnvironment)
        {
            _NarijeDBContext = NarijeDBContext;
            _IHttpContextAccessor = iHttpContextAccessor;
            _IConfiguration = iConfiguration;
            _IHttpClientFactory = iHttpClientFactory;
            _IWebHostEnvironment = iWebHostEnvironment;
        }

        /// <summary>
        /// Index
        /// </summary>
        [AllowAnonymous]
        [Route("index")]
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var Assembly = typeof(Startup).Assembly;
                var LastUpdate = System.IO.File.GetLastWriteTime(Assembly.Location);
                var Version = FileVersionInfo.GetVersionInfo(Assembly.Location).ProductVersion;
                var ProductName = FileVersionInfo.GetVersionInfo(Assembly.Location).ProductName;
                return Ok(new ApiOkResponse(_Message: "Narije Api Running", _Data: new
                {
                    app = new HomeResponse()
                    {
                        ProductName = ProductName,
                        Version = Version,
                        LastUpdate = LastUpdate,
                    }
                }));
            }
            catch (Exception Ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}