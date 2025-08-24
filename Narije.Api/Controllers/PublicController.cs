using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using Narije.Infrastructure.Contexts;
using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using SixLabors.ImageSharp.Processing;

namespace Narije.Api.Controllers
{
    /// <summary>
    /// Home
    /// </summary>
    [Route("public")]
    [AllowAnonymous]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly NarijeDBContext _NarijeDBContext;
        private readonly IHttpContextAccessor _IHttpContextAccessor;
        private readonly IConfiguration _IConfiguration;
        private readonly IHttpClientFactory _IHttpClientFactory;
        private readonly IWebHostEnvironment _IWebHostEnvironment;
        private static string BucketServiceURL = "https://tahlilmobile-gallery.storage.iran.liara.space/";

        /// <summary>
        /// متد سازنده
        /// </summary>
        public PublicController(NarijeDBContext NarijeDBContext, IHttpContextAccessor iHttpContextAccessor, IConfiguration iConfiguration, IHttpClientFactory iHttpClientFactory, IWebHostEnvironment iWebHostEnvironment)
        {
            _NarijeDBContext = NarijeDBContext;
            _IHttpContextAccessor = iHttpContextAccessor;
            _IConfiguration = iConfiguration;
            _IHttpClientFactory = iHttpClientFactory;
            _IWebHostEnvironment = iWebHostEnvironment;

            BucketServiceURL = _IConfiguration.GetSection("baseURL").Value + "/public/downloadFileById/";

        }

        /// <summary>
        /// دریافت دیتای استاتیک سایت
        /// </summary>
        [Route("getStaticsData")]
        [HttpGet]
        public async Task<IActionResult> GetStaticsData()
        {
            try
            {

                var company = await _NarijeDBContext.Settings
                                        .Select(A => new
                                        {
                                            surveyTime = A.SurveyTime,
                                            panelVersion = A.PanelVersion,
                                            frontVersion = A.FrontVersion,
                                            companyName = A.CompanyName,
                                            economicCode = A.EconomicCode,
                                            regNumber = A.RegNumber,
                                            tel = A.Tel,
                                            postalCode = A.PostalCode,
                                            address = A.Address,
                                            forceSurvey = A.ForceSurvey,
                                            nationalId = A.NationalId,
                                            companyGalleryId = A.CompanyGalleryId,
                                            companyDarkGalleryId = A.CompanyDarkGalleryId,
                                            contactMobile = A.ContactMobile,
                                            panelMaintenanceMode= A.PanelMaintenanceMode,
                                            frontMaintenanceMode = A.FrontMaintenanceMode,
                                        })
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync();
                var loginImages = await _NarijeDBContext.LoginImage
               .Select(x => new
               {
                   title = x.Title,
                   description = x.Description,
                   image = x.GalleryId,
                   imageUrl = $"{BucketServiceURL}{x.GalleryId}",
                   forMobile = x.ForMobile
               })
               .AsNoTracking()
               .ToListAsync();



                return Ok(new ApiOkResponse(_Message: "SUCCESS", _Data: new
                {
                    company = company,
                    loginImages = loginImages
                }));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }


        /// <summary>
        /// دریافت فایل 
        /// </summary>
        /// <returns></returns>
        [Route("downloadFileById/{id}")]
        [HttpGet]
        public async Task<IActionResult> DownloadFileById([FromRoute] int id ,  [FromQuery] int? w, [FromQuery] int? h, [FromQuery] int? q = 100)
        {
            var gallery = await _NarijeDBContext.Galleries.Where(A => A.Id == id).AsNoTracking().FirstOrDefaultAsync();

            if (gallery is null)
                return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "فایل یافت نشد"));

            if (gallery.SystemFileName.ToLower().Contains("svg"))
            {
                var contentRoot = _IConfiguration.GetValue<string>(WebHostDefaults.ContentRootKey);
                var filepath2 = "/data/" + string.Format("{0}{1}", gallery.Id, gallery.SystemFileName);

                FileStream fileStream = new FileStream(filepath2, FileMode.Open, FileAccess.Read);
                return File(fileStream, "image/svg+xml; charset=utf-8", gallery.OriginalFileName);
            }
                

            var path = "/data/";
            var filepath = path + string.Format("{0}{1}", gallery.Id, gallery.SystemFileName);

            if (System.IO.File.Exists(filepath) == false)
                return StatusCode(StatusCodes.Status404NotFound, new ApiErrorResponse(_Message: "File not found!"));

            if (w == null && h == null)
            {
                FileStream originalFileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                return File(originalFileStream, "application/octet-stream", gallery.OriginalFileName);
            }

            using (FileStream imageFileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (var image = await SixLabors.ImageSharp.Image.LoadAsync(imageFileStream))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new SixLabors.ImageSharp.Size(w ?? image.Width, h ?? image.Height)
                    }));

                    var encoder = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                    {
                        Quality = q.Value
                    };

                    var memoryStream = new MemoryStream();
                    await image.SaveAsync(memoryStream, encoder);
                    memoryStream.Position = 0;

                    return File(memoryStream, "image/jpeg", gallery.OriginalFileName);
                }
            }
        }

    }

}