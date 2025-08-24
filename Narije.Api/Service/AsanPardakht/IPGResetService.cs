using Newtonsoft.Json;
using Narije.Api.Payment.AsanPardakht.models;
using Narije.Api.Payment.AsanPardakht.models.bill;
using Narije.Api.Payment.AsanPardakht.models.reverse;
using Narije.Api.Payment.AsanPardakht.models.settle;
using Narije.Api.Payment.AsanPardakht.models.verify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using Narije.Infrastructure.Payment;

namespace Narije.Api.Payment.AsanPardakht
{
    public class IPGResetService
    {
        private const string REST_URL = "";
        private readonly IConfiguration _IConfiguration;


        /// <summary>
        /// IPGResetService
        /// </summary>
        /// <param name="iConfiguration"></param>
        public IPGResetService(IConfiguration iConfiguration)
        {
            _IConfiguration = iConfiguration;

        }


        /// <summary>
        /// Token
        /// </summary>
        /// <param name="request"></param>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public async Task<TResult> Token<TRequest, TResult>(TRequest request)
            where TRequest : ITokenCommand
            where TResult : class, ITokenVm, new()

        {
            var refid = string.Empty;
            var _URL = _IConfiguration.GetSection("IPGUrl").Value;
            var client = new HttpClient
            {
                BaseAddress = new Uri(_URL),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", _IConfiguration.GetSection("IPGUser").Value);
            client.DefaultRequestHeaders.Add("pwd", _IConfiguration.GetSection("IPGPassword").Value);
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                var responseMessage = (await client.PostAsync($"v1/Token", content, new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        refid = await responseMessage.Content.ReadAsStringAsync();
                        return new TResult { RefId = JsonConvert.DeserializeObject<string>(refid), ResCode = 0 };
                    case 489:
                        return new TResult { ResCode = 489, ResMessage = "شماره سفارش تکراری است" };
                    case 484:
                        return new TResult { ResCode = 484, ResMessage = "خطای داخلی بانک" };
                    case 486:
                        return new TResult { ResCode = 486, ResMessage = "مبلغ تراکنش در محدوده قرار ندارد" };
                    case 504:
                        refid = await responseMessage.Content.ReadAsStringAsync();
                        return new TResult { ResCode = 504, ResMessage = responseMessage.ReasonPhrase };
                    default:
                        refid = await responseMessage.Content.ReadAsStringAsync();
                        return new TResult { ResCode = (int)responseMessage.StatusCode, ResMessage = "خطای پرداخت بانک شهر" };
                }
            }
            catch (TaskCanceledException)
            {
                return new TResult { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = "پاسخی دریافت نشد" };
            }
            catch (Exception ex)
            {
                return new TResult { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = ex.Message };
            }
        }

        /// <summary>
        /// VerifyTrx
        /// </summary>
        /// <param name="verifyCommand"></param>
        /// <returns></returns>
        public async Task<VerifyVm> VerifyTrx(VerifyCommand verifyCommand)
        {
            var apiUrl = _IConfiguration.GetSection("IPGUrl").Value ?? "";
            var client = new HttpClient
            {
                BaseAddress = new Uri(apiUrl),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", _IConfiguration.GetSection("IPGUser").Value);
            client.DefaultRequestHeaders.Add("pwd", _IConfiguration.GetSection("IPGPassword").Value);
            var content = new StringContent(JsonConvert.SerializeObject(verifyCommand), Encoding.UTF8, "application/json");

            try
            {
                var responseMessage = (await client.PostAsync($"v1/Verify", content, new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        return new VerifyVm() { ResCode = 0, ResMessage = "تراکنش تایید شد" };
                    case 400:
                        return new VerifyVm() { ResCode = 400, ResMessage = "در خواست نامعتبر" };//"bad request" };
                    case 401:
                        return new VerifyVm() { ResCode = 401, ResMessage = "خطای دسترسی" };//"unauthorized. probably wrong or unsent header(s)" };
                    case 471:
                        return new VerifyVm() { ResCode = 471, ResMessage = "تراکنش موفق نبوده است" };//"original transaction was failed" };
                    case 472:
                        return new VerifyVm() { ResCode = 472, ResMessage = "تراکنش تایید نشد" };//"transaction not verified" };
                    case 473:
                        return new VerifyVm() { ResCode = 473, ResMessage = "تراکنش کنسل شده است" };//"transaction already requested for reversal" };
                    case 474:
                        return new VerifyVm() { ResCode = 474, ResMessage = "تراکنش تغییر کرده است" };//"transaction already requested for reconcilation" };
                    case 475:
                        return new VerifyVm() { ResCode = 475, ResMessage = "تراکنش در لیست بازگشت مبلغ است" };//"transaction already listed for reversal" };
                    case 476:
                        return new VerifyVm() { ResCode = 476, ResMessage = "تراکنش در لیست اصلاح قرار دارد" };//"transaction already listed for reconcilation" };
                    case 477:
                        return new VerifyVm() { ResCode = 477, ResMessage = "بانک صادر کننده تراکنش را تایید نمی کند" };//"identity not trusted to proceed" };
                    case 478:
                        return new VerifyVm() { ResCode = 478, ResMessage = "این تراکنش کنسل شده است" };//"verification already cancelled" };
                    case 571:
                        return new VerifyVm() { ResCode = 571, ResMessage = "تراکنش هنوز انجام نشده است" };//"not yet processed" };
                    case 572:
                        return new VerifyVm() { ResCode = 572, ResMessage = "وضعیت تراکنش قابل استعلام نیست" };//"transaction status undetermined" };
                    case 573:
                        return new VerifyVm() { ResCode = 573, ResMessage = "خطای داخلی بانک" };//"unable to request for reconcilation due to an internal error" };
                    default:
                        return new VerifyVm() { ResCode = (int)responseMessage.StatusCode, ResMessage = responseMessage.ReasonPhrase };
                }
            }
            catch (TaskCanceledException)
            {
                return new VerifyVm() { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = "پاسخی دریافت نشد" };
            }
        }

        /// <summary>
        /// SettleTrx
        /// </summary>
        /// <param name="settleCommand"></param>
        /// <returns></returns>
        public async Task<SettleVm> SettleTrx(SettleCommand settleCommand)
        {
            var apiUrl = _IConfiguration.GetSection("IPGUrl").Value ?? "";
            var client = new HttpClient
            {
                BaseAddress = new Uri(apiUrl),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", _IConfiguration.GetSection("IPGUser").Value);
            client.DefaultRequestHeaders.Add("pwd", _IConfiguration.GetSection("IPGPassword").Value);
            var content = new StringContent(JsonConvert.SerializeObject(settleCommand), Encoding.UTF8, "application/json");

            try
            {
                var responseMessage = (await client.PostAsync($"v1/Settlement", content, new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        return new SettleVm() { ResCode = 0, ResMessage = "موفق" };//"settlement request succeeded" };
                    case 400:
                        return new SettleVm() { ResCode = 400, ResMessage = "در خواست نامعتبر" };//"bad request" };
                    case 401:
                        return new SettleVm() { ResCode = 401, ResMessage = "" };//"unauthorized. probably wrong or unsent header(s)" };
                    case 471:
                        return new SettleVm() { ResCode = 471, ResMessage = "تراکنش موفق نبوده است" };//"original transaction was failed" };
                    case 472:
                        return new SettleVm() { ResCode = 472, ResMessage = "تراکنش تایید نشد" };//"transaction not verified" };
                    case 473:
                        return new SettleVm() { ResCode = 473, ResMessage = "تراکنش کنسل شده است" };//"transaction already requested for reversal" };
                    case 474:
                        return new SettleVm() { ResCode = 474, ResMessage = "تراکنش تغییر کرده است" };//"transaction already requested for reconcilation" };
                    case 475:
                        return new SettleVm() { ResCode = 475, ResMessage = "تراکنش در لیست بازگشت مبلغ است" };//"transaction already listed for reversal" };
                    case 476:
                        return new SettleVm() { ResCode = 476, ResMessage = "تراکنش در لیست اصلاح قرار دارد" };//"transaction already listed for reconcilation" };
                    case 477:
                        return new SettleVm() { ResCode = 477, ResMessage = "بانک صادر کننده تراکنش را تایید نمی کند" };//"identity not trusted to proceed" };
                    case 478:
                        return new SettleVm() { ResCode = 478, ResMessage = "این تراکنش کنسل شده است" };//"verification already cancelled" };
                    case 571:
                        return new SettleVm() { ResCode = 571, ResMessage = "تراکنش هنوز انجام نشده است" };//"not yet processed" };
                    case 572:
                        return new SettleVm() { ResCode = 572, ResMessage = "وضعیت تراکنش قابل استعلام نیست" };//"transaction status undetermined" };
                    case 573:
                        return new SettleVm() { ResCode = 573, ResMessage = "خطای داخلی بانک" };//"unable to request for reconcilation due to an internal error" };
                    default:
                        return new SettleVm() { ResCode = (int)responseMessage.StatusCode, ResMessage = responseMessage.ReasonPhrase };
                }
            }
            catch (TaskCanceledException)
            {
                return new SettleVm() { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = "پاسخی دریافت نشد دریافت نشد" };
            }
        }

        /// <summary>
        /// ReverseTrx
        /// </summary>
        /// <param name="reverseCommand"></param>
        /// <returns></returns>
        public async Task<ReverseVm> ReverseTrx(ReverseCommand reverseCommand)
        {
            var apiUrl = _IConfiguration.GetSection("IPGUrl").Value ?? "";
            var client = new HttpClient
            {
                BaseAddress = new Uri(apiUrl),
                Timeout = TimeSpan.FromSeconds(20)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("usr", _IConfiguration.GetSection("IPGUser").Value);
            client.DefaultRequestHeaders.Add("pwd", _IConfiguration.GetSection("IPGPassword").Value);
            var content = new StringContent(JsonConvert.SerializeObject(reverseCommand), Encoding.UTF8, "application/json");

            try
            {
                var responseMessage = (await client.PostAsync($"v1/Reverse", content,
                                       new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));
                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        return new ReverseVm() { ResCode = 0, ResMessage = "موفق" };//"reversing  request succeeded" };
                    case 400:
                        return new ReverseVm() { ResCode = 400, ResMessage = "در خواست نامعتبر" };//"bad request" };
                    case 401:
                        return new ReverseVm() { ResCode = 401, ResMessage = "" };//"unauthorized. probably wrong or unsent header(s)" };
                    case 471:
                        return new ReverseVm() { ResCode = 471, ResMessage = "تراکنش موفق نبوده است" };//"original transaction was failed" };
                    case 472:
                        return new ReverseVm() { ResCode = 472, ResMessage = "تراکنش تایید نشد" };//"transaction not verified" };
                    case 473:
                        return new ReverseVm() { ResCode = 473, ResMessage = "تراکنش کنسل شده است" };//"transaction already requested for reversal" };
                    case 474:
                        return new ReverseVm() { ResCode = 474, ResMessage = "تراکنش تغییر کرده است" };//"transaction already requested for reconcilation" };
                    case 475:
                        return new ReverseVm() { ResCode = 475, ResMessage = "تراکنش در لیست بازگشت مبلغ است" };//"transaction already listed for reversal" };
                    case 476:
                        return new ReverseVm() { ResCode = 476, ResMessage = "تراکنش در لیست اصلاح قرار دارد" };//"transaction already listed for reconcilation" };
                    case 477:
                        return new ReverseVm() { ResCode = 477, ResMessage = "بانک صادر کننده تراکنش را تایید نمی کند" };//"identity not trusted to proceed" };
                    case 478:
                        return new ReverseVm() { ResCode = 478, ResMessage = "این تراکنش کنسل شده است" };//"verification already cancelled" };
                    case 571:
                        return new ReverseVm() { ResCode = 571, ResMessage = "تراکنش هنوز انجام نشده است" };//"not yet processed" };
                    case 572:
                        return new ReverseVm() { ResCode = 572, ResMessage = "وضعیت تراکنش قابل استعلام نیست" };//"transaction status undetermined" };
                    case 573:
                        return new ReverseVm() { ResCode = 573, ResMessage = "خطای داخلی بانک" };//"unable to request for reconcilation due to an internal error" };
                    default:
                        return new ReverseVm() { ResCode = (int)responseMessage.StatusCode, ResMessage = responseMessage.ReasonPhrase };
                }
            }
            catch (TaskCanceledException)
            {
                return new ReverseVm() { ResCode = (int)HttpStatusCode.GatewayTimeout, ResMessage = "پاسخی دریافت نشد" };//"Gateway Timeout" };
            }
        }

        /// <summary>
        /// TestTran
        /// </summary>
        /// <param name="merchantConfigId"></param>
        /// <param name="localInvoiceId"></param>
        /// <returns></returns>
        public async Task<PaymentResultVm> TranResult(int merchantConfigId, long localInvoiceId)
        {
            var apiUrl = _IConfiguration.GetSection("IPGUrl").Value;
            var usr = _IConfiguration.GetSection("IPGUser").Value;
            var pwd = _IConfiguration.GetSection("IPGPassword").Value;

            using var client = new HttpClient();
            try
            {
                // Construct the URL with query parameters
                var requestUrl = $"{apiUrl}v1/TranResult?" +
                                 $"LocalInvoiceId={localInvoiceId}&" +
                                 $"MerchantConfigurationId={merchantConfigId}";

                // Add headers to the request
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Add("usr", usr);
                client.DefaultRequestHeaders.Add("pwd", pwd);

                // Make the GET request asynchronously
                var response = await client.GetAsync(requestUrl, new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var transactionResult = JsonConvert.DeserializeObject<TransactionResultVm>(responseBody);

                    switch ((int)response.StatusCode)
                    {
                        case 200:
                            var paymentResult = new PaymentResultVm
                            {
                                ResMessage = response.ReasonPhrase,
                                ResCode = 0,
                                Rrn = transactionResult.Rrn,
                                CardNumber = transactionResult.CardNumber,
                                RefId = transactionResult.RefID,
                                Amount = string.IsNullOrWhiteSpace(transactionResult.Amount) ? 0 : decimal.Parse(transactionResult.Amount),
                                PayGateTranID = string.IsNullOrWhiteSpace(transactionResult.PayGateTranID) ? 0 : long.Parse(transactionResult.PayGateTranID)
                            };
                            return paymentResult;

                        case 472:
                            return new PaymentResultVm() { ResCode = 472, ResMessage = "اطلاعات تراکنش در بانک یافت نشد" };//"no records found" };
                        case 408:
                            return new PaymentResultVm() { ResCode = 471, ResMessage = "بانک تراکنش را تایید نکرد" };//"identity not trusted to proceed" };
                        case 504:
                            return new PaymentResultVm() { ResCode = 401, ResMessage = "خطا در مجوز دسترسی" };//"unauthorized. probably wrong or unsent header(s)" };
                        case 571:
                            return new PaymentResultVm() { ResCode = 571, ResMessage = "خطا در پردازش درخواست" };//"error in processing" };
                        default:
                            return new PaymentResultVm() { ResCode = (short)response.StatusCode, ResMessage = response.ReasonPhrase };
                    }

                    
                }
                else
                {
                    // Deserialize the error response into ErrorResponse class
                    var errorContent = await response.Content.ReadAsStringAsync();

                    var errorResponse = JsonConvert.DeserializeObject<ErrorDetails>(errorContent);
                    return new PaymentResultVm() { ResCode = 500, ResMessage = errorResponse.Message };
                }
            }
            catch (Exception ex)
            {
                // Handle the exception as needed, e.g., return a default result or rethrow the exception
                return new PaymentResultVm() { ResCode = 500, ResMessage = "An error occurred during the request." };
            }
        }

        /// <summary>
        /// TranResult
        /// </summary>
        /// <param name="merchantConfigId"></param>
        /// <param name="localInvoiceId"></param>
        /// <returns></returns>
        public async Task<PaymentResultVm> TranResultOld(int merchantConfigId, long localInvoiceId)
        {
            var apiUrl = _IConfiguration.GetSection("IPGUrl").Value;
            var client = new HttpClient
            {
                BaseAddress = new Uri(REST_URL),
                Timeout = TimeSpan.FromSeconds(20)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            client.DefaultRequestHeaders.Add("usr", _IConfiguration.GetSection("IPGUser").Value);
            client.DefaultRequestHeaders.Add("pwd", _IConfiguration.GetSection("IPGPassword").Value);
            PaymentResultVm paymentResultVm;
            try
            {
                var result = await client.GetAsync($"v1/TranResult?LocalInvoiceId={localInvoiceId}" +
                                                   $"&MerchantConfigurationId={merchantConfigId}",
                    new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token);

                var responseMessage = (await client.GetAsync($"v1/TranResult?LocalInvoiceId={localInvoiceId}" +
                                                             $"&MerchantConfigurationId={merchantConfigId}",
                                                              new CancellationTokenSource(TimeSpan.FromSeconds(20)).Token));

                switch ((int)responseMessage.StatusCode)
                {
                    case 200:
                        var content = await responseMessage.Content.ReadAsStringAsync();

                        paymentResultVm = JsonConvert.DeserializeObject<PaymentResultVm>(content);
                        paymentResultVm.ResCode = 0;
                        paymentResultVm.ResMessage = responseMessage.ReasonPhrase;

                        return paymentResultVm;
                    case 472:
                        return new PaymentResultVm() { ResCode = 472, ResMessage = "اطلاعات تراکنش در بانک یافت نشد" };//"no records found" };
                    case 408:
                        return new PaymentResultVm() { ResCode = 471, ResMessage = "بانک تراکنش را تایید نکرد" };//"identity not trusted to proceed" };
                    case 504:
                        return new PaymentResultVm() { ResCode = 401, ResMessage = "خطا در مجوز دسترسی" };//"unauthorized. probably wrong or unsent header(s)" };
                    case 571:
                        return new PaymentResultVm() { ResCode = 571, ResMessage = "خطا در پردازش درخواست" };//"error in processing" };
                    default:
                        return new PaymentResultVm() { ResCode = (short)responseMessage.StatusCode, ResMessage = responseMessage.ReasonPhrase };
                }
            }
            catch (TaskCanceledException)
            {
                return new PaymentResultVm() { ResCode = 408, ResMessage = "پاسخی دریافت نشد" };//"Timeout By WebPay Application" };
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                }
                return new PaymentResultVm() { ResCode = 500, ResMessage = ex.Message };
            }

        }
    }
}