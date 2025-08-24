using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Api.Payment.ZarinPal.Models
{
    public class PaymentVerification
    {
        public string amount { set; get; }
        public string merchant_id { set; get; }
        public string authority { set; get; }

    }
}
