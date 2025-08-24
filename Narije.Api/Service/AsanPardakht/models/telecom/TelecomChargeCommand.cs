using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Narije.Api.Payment.AsanPardakht.models.sale;

namespace Narije.Api.Payment.AsanPardakht.models.telecom
{
    public class TelecomChargeCommand : SaleCommand
    {
        public TelecomChargeCommand(int merchantConfigurationId, TelecomChargeServiceType serviceType, long orderId, ulong amountInRials, string callbackURL
            , string destinationMobile, int productId)
            : base(merchantConfigurationId, (int)serviceType, orderId, amountInRials, callbackURL, JsonConvert.SerializeObject(new
            {
                destinationMobile,
                productId
            }))
        {
        }
    }
}