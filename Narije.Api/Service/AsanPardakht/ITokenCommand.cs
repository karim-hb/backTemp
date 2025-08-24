using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Api.Payment.AsanPardakht
{
    public interface ITokenCommand 
    {
         int merchantConfigurationId { get; set; }
         int serviceTypeId { get; set; }
         long localInvoiceId { get; set; }
         ulong amountInRials { get; set; }
         string callbackURL { get; set; }
         string additionalData { get; set; }
         string localDate { get ; } 
         string paymentId { get; }
    }
}