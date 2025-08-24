using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Narije.Api.Payment.AsanPardakht.models.sale;

namespace Narije.Api.Payment.AsanPardakht.models.bill
{
    public class BillCommand : SaleCommand
    {
        public BillCommand(int merchantConfigurationId, int serviceTypeId, long orderId, ulong amountInRials, string callbackURL
            , string billId, string payId)
            : base(merchantConfigurationId, serviceTypeId, orderId, amountInRials, callbackURL, JsonConvert.SerializeObject(new
            {
                billId = billId,
                payId = payId
            }))
        {
        }
    }
}