using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Narije.Api.Payment.AsanPardakht.models.verify
{
    public class VerifyCommand
    {
        public int merchantConfigurationId { get; set; }
        public ulong payGateTranId { get; set; }
    }
}