
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Api.Payment.ZarinPal.Models
{
    public class PaymentResponse
    {
        public String authority { set; get; }
        public int code { set; get; }
        public int fee { set; get; }
        public String message { set; get; }

    }
}
