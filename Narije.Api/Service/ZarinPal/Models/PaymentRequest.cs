using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace Narije.Api.Payment.ZarinPal.Models
{


    public class PaymentRequest
    {
        public string merchant_id { get; set; }

        public string amount { get; set; }
        public string description { get; set; }
        public string callback_url { get; set; }


        public PaymentRequest(string merchant_id, string amount, string description, string callback_url)
        {
            this.merchant_id = merchant_id;
            this.amount = amount;
            this.description = description;
            this.callback_url = callback_url;

        }


    }

}
