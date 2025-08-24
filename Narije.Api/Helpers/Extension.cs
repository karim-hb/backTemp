using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using mpNuget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Narije.Core.DTOs.Enum;
using Narije.Core.DTOs.Public;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;

namespace Narije.Api.Helpers
{
    /// <summary>
    /// متد های الحاقی
    /// </summary>
    public static class Extension
    {
        public static IActionResult ServiceReturn(this ControllerBase controller, ApiResponse response)
        {
            if (response.Code == 200)
            {
                return controller.Ok(response);
            }
            else if (response.Code >= 300 && response.Code <= 399)
            {
                return controller.StatusCode(response.Code, new Api300Response(_Code: response.Code, _Url: response.Message));
            }
            else
            {
                return controller.StatusCode(response.Code, new ApiErrorResponse(_Code: response.Code, _Message: response.Message));
            }

        }

        /// <summary>
        /// لاگ خطا
        /// </summary>
        public static async Task<int> LogError(Exception e, NarijeDBContext _NarijeDBContext, IHttpContextAccessor _IHttpContextAccessor)
        {
            //var log = new ErrorLog();

            try
            {
                /*
                log.DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time"));
                if (e.InnerException != null)
                    log.Error = e.InnerException.Message;
                else
                    log.Error = e.Message;
                log.Url = _IHttpContextAccessor.HttpContext.Request.Method + " >> " + _IHttpContextAccessor.HttpContext.Request.Path;
                if (_IHttpContextAccessor.HttpContext.Request.Form != null)
                {
                    log.Request = JsonConvert.SerializeObject(_IHttpContextAccessor.HttpContext.Request.Form);
                }
                else
                {
                    log.Request = _IHttpContextAccessor.HttpContext.Request.Query.Keys.ToString();
                }
                log.SequenceId = "-";
                await _NarijeDBContext.ErrorLogs.AddAsync(log);
                await _NarijeDBContext.SaveChangesAsync();
                */
                string log = "[" + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")).ToString() + "] " + _IHttpContextAccessor.HttpContext.Request.Host.Value + "\n\r";
                    
                log = log + _IHttpContextAccessor.HttpContext.Request.Method + " >> " + _IHttpContextAccessor.HttpContext.Request.Path + "\n\r";
                if (_IHttpContextAccessor.HttpContext.Request.Form != null)
                {
                    log = log + JsonConvert.SerializeObject(_IHttpContextAccessor.HttpContext.Request.Form);
                }
                else
                {
                    log = log + _IHttpContextAccessor.HttpContext.Request.Query.Keys.ToString();
                }

                if (e.InnerException != null)
                    log = log + "\n\r" + e.InnerException.Message;
                else
                    log = log + "\n\r" + e.Message;

                /*
                 GeneralShopAdvisor@gmail.com
                 password: Masoud09129347981
                */

                MailMessage msg = new MailMessage("GeneralShopAdvisor@gmail.com", "masoudprg@gmail.com")
                {
                    Subject = "TM Error Log",
                    Body = log
                };
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential("GeneralShopAdvisor@gmail.com", "gyamobdzkrnzeket");
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                //smtp.SendAsync(msg, new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token);
                smtp.Send(msg);

            }
            catch (Exception ex)
            {

            }
            return 0;

        }

        /// <summary>
        /// دریافت تاریخ های بین بازه
        /// </summary>
        public static List<DateTime> GetDatesBetween(DateTime StartDate, DateTime EndDate)
        {
            List<DateTime> AllDates = new();
            for (DateTime Date = StartDate; Date <= EndDate; Date = Date.AddDays(1))
                AllDates.Add(Date);
            return AllDates;
        }

        /// <summary>
        /// ارسال پیامک
        /// </summary>
        public static async Task<string> SendSms(this IHttpClientFactory _IHttpClientFactory, string Mobile, string Message)
        {
            object input = new
            {
                UserName = "",
                Password = "",
                To = Mobile,
                From = "", // شماره اختصاصی
                Text = Message,
            };
            var ClientFactory = _IHttpClientFactory.CreateClient();
            HttpContent _HttpContent = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            var Response = await ClientFactory.PostAsync("https://rest.Sms.ir/api/SendSMS", _HttpContent);
            var Result = await Response.Content.ReadAsStringAsync();
            return Result;
        }

        /// <summary>
        /// تابع صفحه بندی
        /// </summary>
        public async static Task<PagedResult<T>> GetPaged<T>(this IQueryable<T> query, int Page, int Limit) where T : class
        {
            var Meta = new MetaResult()
            {
                CurrentPage = Page,
                Limit = Limit,
                Total = await query.CountAsync(),
                TotalInPage = Limit,
                Prev = Page - 1
            };

            if (Limit == 0)
                Limit = 100;
            var PageCount = (double)Meta.Total / Limit;
            Meta.TotalPage = (int)Math.Ceiling(PageCount);

            if (Meta.Prev < 0)
                Meta.Prev = null;

            var Result = new PagedResult<T>
            {
                Data = await query.Skip((Page - 1) * Limit).Take(Limit).ToListAsync(),
                Meta = Meta
            };

            return Result;
        }

        /// <summary>
        /// تابع صفحه بندی
        /// </summary>
        public static PagedResult<T> GetPaged2<T>(this IQueryable<T> query, int Page, int Limit) where T : class
        {
            var Meta = new MetaResult()
            {
                CurrentPage = Page,
                Limit = Limit,
                Total = query.Count(),
                TotalInPage = Limit,
                Prev = Page - 1
            };

            if (Limit == 0)
                Limit = 100;
            var PageCount = (double)Meta.Total / Limit;
            Meta.TotalPage = (int)Math.Ceiling(PageCount);

            if (Meta.Prev < 0)
                Meta.Prev = null;

            var Result = new PagedResult<T>
            {
                Data = query.Skip((Page - 1) * Limit).Take(Limit).ToList(),
                Meta = Meta
            };

            return Result;
        }

        /// <summary>
        /// افزودن سورت به کوئری
        /// </summary>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ColumnName, bool IsAscending = true)
        {
            if (string.IsNullOrEmpty(ColumnName))
            {
                return source;
            }

            ParameterExpression parameter = Expression.Parameter(source.ElementType, "");

            MemberExpression property = Expression.Property(parameter, ColumnName);
            LambdaExpression lambda = Expression.Lambda(property, parameter);

            string methodName = IsAscending ? "OrderBy" : "OrderByDescending";

            Expression methodCallExpression = Expression.Call(typeof(Queryable), methodName,
                                  new Type[] { source.ElementType, property.Type },
                                  source.Expression, Expression.Quote(lambda));

            return source.Provider.CreateQuery<T>(methodCallExpression);
        }



        /// <summary>
        /// تبدیل رشته به TimeSpan
        /// </summary>
        public static TimeSpan ConvertStringToTimeSpan(string Value)
        {
            var S = Value.Split(':');
            if (S.Length == 0)
                return TimeSpan.Zero;

            if (S.Length == 2)
            {
                var Hours = Convert.ToInt32(S[0]);
                var Minutes = Convert.ToInt32(S[1]);
                var TotalMinutes = Hours * 60 + Minutes;
                return new TimeSpan(hours: 0, minutes: TotalMinutes, seconds: 0);
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// دریافت روز هفته شمسی از تاریخ میلادی
        /// </summary>
        public static string ConvertDateToPersianDayOfWeek(DateTime Value)
        {
            return Value.DayOfWeek switch
            {
                DayOfWeek.Saturday => "شنبه",
                DayOfWeek.Sunday => "یکشنبه",
                DayOfWeek.Monday => "دوشنبه",
                DayOfWeek.Tuesday => "سه شنبه",
                DayOfWeek.Wednesday => "چهارشنبه",
                DayOfWeek.Thursday => "پنج شنبه",
                DayOfWeek.Friday => "جمعه",
                _ => "-",
            };
        }

        /// <summary>
        /// تبدیل عدد صحیح به ساعت و دقیقه
        /// </summary>
        public static string ConvertNumberToTime(int Value)
        {
            //if (Value <= 0)
            //    return string.Empty;

            var Time = new TimeSpan(hours: 0, minutes: Math.Abs(Value), seconds: 0);
            return string.Format("{0:00}:{1:00}", Math.Floor(Time.TotalHours), Time.Minutes) + ((Value < 0) ? "- " : "");
        }

        /// <summary>
        /// تبدیل عدد صحیح به روز و ساعت و دقیقه
        /// </summary>
        public static string ConvertNumberToDayTime(int Value, int DayLen)
        {
            if (Value <= 0)
                return string.Empty;
            if (DayLen == 0)
                return string.Empty;
            var Time = new TimeSpan(hours: 0, minutes: Value % DayLen, seconds: 0);
            return string.Format("{0} روز و ", (int)(Value / DayLen)) + string.Format("{0:00}:{1:00}", Time.Hours, Time.Minutes);
        }

        /// <summary>
        /// تبدیل عدد صحیح به روز و ساعت و دقیقه
        /// </summary>
        public static string ConvertNumberToDayHourMin(int Value)
        {
            int DayLen = 8*60;
            var Time = new TimeSpan(hours: 0, minutes: Value % DayLen, seconds: 0);
            return string.Format("{0}.{1}.{2}", (int)(Value / DayLen), Time.Hours, Time.Minutes);
        }

        /// <summary>
        /// تبدیل کوئری استرینگ به مدل
        /// </summary>
        public static ParseQueryStringModel ConvertQueryStringToModel(IHttpContextAccessor _IHttpContextAccessor)
        {
            ParseQueryStringModel PQS = new();
            var Param = HttpUtility.ParseQueryString(_IHttpContextAccessor.HttpContext.Request.QueryString.ToString());
            var AllKey = Param.AllKeys.Where(A => A.StartsWith("filter") && A.EndsWith("[key]")).ToList();
            foreach (var Key in AllKey)
            {
                List<object> Values = new();
                var Str = $"{Key.Replace("[key]", string.Empty)}[value]";
                var AllValue = Param.AllKeys.Where(A => A.StartsWith(Str)).ToList();
                foreach (var Value in AllValue)
                {
                    if (!string.IsNullOrEmpty(Param[Value]))
                        Values.Add(Param[Value]);
                }

                if (Values.Count > 0)
                {
                    PQS.FilterGroups.Add(new FilterGroupModel()
                    {
                        Key = Param[Key.ToString()],
                        Values = Values
                    });
                }
            }

            return PQS;
        }

    }
}