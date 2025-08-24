using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Narije.Api.Payment.ZarinPal.Models
{
    public class VerificationResponse
    {

        public int code { get; set; }
        public string message { get; set; }
        public string card_pan { get; set; }
        public ulong ref_id { get; set; }
    }

}
