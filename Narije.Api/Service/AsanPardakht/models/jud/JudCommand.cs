using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Narije.Api.Payment.AsanPardakht.models.jud
{
    public class JudCommand : ITokenCommand
    {
        public JudCommand(
            int merchantConfigurationId,
            int serviceTypeId, 
            long orderId,
            ulong amountInRials,
            string callbackURL,
            string additionalData)
        {
            this.merchantConfigurationId = merchantConfigurationId;
            this.serviceTypeId = serviceTypeId;
            this.localInvoiceId = orderId;
            this.amountInRials = amountInRials;
            this.callbackURL = callbackURL;
            this.additionalData = additionalData;
        }
        public int merchantConfigurationId { get; set; }
        public int serviceTypeId { get; set; }
        public long localInvoiceId { get; set; }
        public ulong amountInRials { get; set; }
        public string callbackURL { get; set; }
        public string additionalData { get; set; }
        public string localDate { get { return DateTime.Now.ToString("yyyyMMdd HHmmss"); } }
        public string paymentId { get { return "0"; } }
    }
}