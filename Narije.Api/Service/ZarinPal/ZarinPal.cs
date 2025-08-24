using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Narije.Api.Payment.ZarinPal.Models;

namespace Narije.Api.Payment.ZarinPal
{
    public class ZarinPal
    {

        private const String PAYMENT_REQ_URL = "https://api.zarinpal.com/pg/v4/payment/request.json";
        private const String PAYMENT_PG_URL = "https://www.zarinpal.com/pg/StartPay/";
        private const String PAYMENT_VERIFICATION_URL = "https://api.zarinpal.com/pg/v4/payment/verify.json";


        public async Task<string> PaymentRequest(PaymentRequest PaymentRequest)
        {
            var client = new HttpClient();

            var json = JsonConvert.SerializeObject(PaymentRequest);

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(PAYMENT_REQ_URL, content);

            string responseBody = await response.Content.ReadAsStringAsync();


            JObject jodata = JObject.Parse(responseBody);

            string dataauth = jodata["data"].ToString();

            PaymentResponse _Response = JsonConvert.DeserializeObject<PaymentResponse>(dataauth);

            if (_Response.code != 100)
                return null;

            var str = _Response.authority;

            return str;
        }

        public string CreateUrl(string Authority)
        {
            return PAYMENT_PG_URL + Authority;

        }

        public async Task<VerificationResponse> PaymentVerification(PaymentVerification parameters)
        {
            var client = new HttpClient();

            var json = JsonConvert.SerializeObject(parameters);

            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(PAYMENT_VERIFICATION_URL, content);

            string responseBody = await response.Content.ReadAsStringAsync();

            JObject jodata = JObject.Parse(responseBody);

            string data = jodata["data"].ToString();

            if (data.Equals("[]"))
                return null;
            else
            {
                VerificationResponse _Response = JsonConvert.DeserializeObject<VerificationResponse>(data);

                return _Response;
            }
        }


        public VerificationResponse InvokePaymentVerificationWithExtra(PaymentVerification verificationRequest)
        {
            /*
            URLs url = new URLs(this.IsSandBox,true);
            _HttpCore.URL = url.GetVerificationURL();
            _HttpCore.Method = Method.POST;
            _HttpCore.Raw = verificationRequest;


            String response = _HttpCore.Get();
            JavaScriptSerializer j = new JavaScriptSerializer();
            VerificationResponse verification = j.Deserialize<VerificationResponse>(response);
            */

            return null;

        }

    }

}