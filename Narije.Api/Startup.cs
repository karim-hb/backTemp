using Hangfire;
using Hangfire.SqlServer;
using HangfireBasicAuthenticationFilter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.WebEncoders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Tikment.Api.Helpers;
using Narije.Api.Helpers;
using Narije.Core;
using Narije.Infrastructure.Contexts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using TikmentApi.Infrastructure;
using Serilog;
using Narije.Api.Jobs;
using Narije.Core.Interfaces.GenericRepository;
using Narije.Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.FileProviders;

namespace Narije.Api
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Property
        /// </summary>
        public IConfiguration _IConfiguration { get; }

        /// <summary>
        /// Startup
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            _IConfiguration = configuration;
        }



        /// <summary>
        /// ConfigureServices
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            #region فعال سازی IIS
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            #endregion
          
            #region فعال سازی یونیکد
            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
            });
            #endregion

            #region فعال سازی فشرده سازی
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            // فعال سازی فشرده سازی
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes =
                    ResponseCompressionDefaults.MimeTypes.Concat(
                        new[] {
                            "text/plain",
                            "text/html",
                            "text/xml",
                            "text/css",
                            "text/json",
                            "application/xml",
                            "application/javascript",
                            "application/json",
                            "font/woff2",
                            "image/svg+xml",
                            "image/x-icon",
                            "image/png"});
            });
            #endregion

            #region افزودن سولوشن های پایه
            services.AddSolutionCore();
            services.AddSolutionInfrastructure(_IConfiguration);
            #endregion

            #region احراز هویت
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                var key = Encoding.UTF8.GetBytes(_IConfiguration["Jwt:Key"]);

                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_IConfiguration["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    RequireExpirationTime = false
                };
            });
            #endregion

            #region افزودن MVC

         

            services.AddControllers()
      .AddNewtonsoftJson(options =>
      {
          options.SerializerSettings.ContractResolver = new DefaultContractResolver();
          options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

      });
            services.AddHttpContextAccessor();

            services.AddHttpClient();
            #endregion

            #region  تنظیمات دیتابیس
            services.AddDbContext<NarijeDBContext>(option => option.UseSqlServer(_IConfiguration.GetConnectionString("Conn"))
                    .UseLazyLoadingProxies()
                    .LogTo(message => Debug.WriteLine(message), LogLevel.Debug, DbContextLoggerOptions.DefaultWithLocalTime | DbContextLoggerOptions.SingleLine)
                    .EnableSensitiveDataLogging());
            /*
                     sqlServerOptionsAction: sqlOptions =>
                     {
                         sqlOptions.EnableRetryOnFailure(
                             maxRetryCount: 10,
                             maxRetryDelay: TimeSpan.FromSeconds(5),
                             errorNumbersToAdd: null
                             );
                     })
            */

            #endregion

            #region افزودن مدیریت جاب
            var options = new SqlServerStorageOptions
            {
                SlidingInvisibilityTimeout = TimeSpan.FromDays(30),
                QueuePollInterval = TimeSpan.Zero
            };

            services.AddHangfire(config => config.UseSqlServerStorage(_IConfiguration.GetConnectionString("Conn"), options));
            services.AddHangfireServer();

            services.AddControllers();
            #endregion

            #region افزودن نسخه بندی سرویس ها
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
                                                                new HeaderApiVersionReader("x-api-version"),
                                                                new MediaTypeApiVersionReader("x-api-version"));
            });

            /*
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            */
            #endregion

            #region افزودن داکیومنت سرویس
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API",
                });

                c.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "API V2",
                });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                                         {
                                             { jwtSecurityScheme, Array.Empty<string>() }
                                         });
                
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                
            });
            #endregion

            #region افزودن Cors
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
            #endregion

            #region Serilog            

            //Add support to logging with SERILOG
            //services.AddSerilog();

            #endregion
      
         
            #region AntiForgery 
            /*
            services.AddAntiforgery(options =>
            {
                // Set Cookie properties using CookieBuilder properties†.
                options.FormFieldName = "AntiforgeryFieldname";
                options.HeaderName = "X-CSRF-TOKEN-HEADERNAME";
                options.SuppressXFrameOptionsHeader = false;
            });
            */
            #endregion

            #region memoryCatch
            services.AddMemoryCache();
            #endregion
        }

        /// <summary>
        /// Configure
        /// </summary>
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseMiddleware<BasicAuthMiddleware>();

                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                    c.SwaggerEndpoint("/swagger/v2/swagger.json", "My API V2");
                    c.RoutePrefix = string.Empty;
                });
            }

      
            app.UseHangfireDashboard("/njdashboard", new DashboardOptions()
            {
                AppPath = null,
                DashboardTitle = "Narijeh Dashboard Jobs",
                Authorization = new[]{
                new HangfireCustomBasicAuthenticationFilter{
                    User ="admin",
                    Pass = "Sonyvpcsa"
                }
            }
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider("/data"),
                RequestPath = "/images",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
                    ctx.Context.Response.Headers.Append("Expires", DateTime.UtcNow.AddDays(7).ToString("R"));
                }
            });

            app.UseMiddleware<GalleryMiddleware>();

            app.UseRouting();

            app.UseCors("MyPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            //app.UseSerilogRequestLogging();

            //app.UseMiddleware<LogMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id:int?}");
            });

            app.UseExceptionHandler(builder =>
            {
                builder.Run(async context =>
                {
                    var error = context.Features.Get<IExceptionHandlerFeature>();
                    if (error != null)
                    {
                        var ex = error.Error;
                        Console.WriteLine($"Unhandled Exception: {ex.Message}\n{ex.StackTrace}");
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsJsonAsync(new { ex.Message, ex.StackTrace });
                    }
                });
            });
          
        }
    }
}