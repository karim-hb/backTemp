using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Narije.Api.Payment.AsanPardakht
{
    public enum ServiceTypeEnum : int
    {
        [Description("خرید")]
        Sale = 1,
        [Description("قبض")]
        Bill = 3,
        [Description("شارژ")]
        Topup = 6,
        [Description("بسته اینترنت")]
        Bolton = 7,
        [Description("استعلام قوه")]
        Jud = 50
    }
}