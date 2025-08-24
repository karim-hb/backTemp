using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.WalletPayment
{
    public class WalletPaymentEditRequest
    {
        public Int32 id { get; set; }
        public Int32 op { get; set; }
        public Int32 userId { get; set; }
        public Int64 value { get; set; }
        public String refNumber { get; set; }
        public String result { get; set; }
        public String pan { get; set; }
        public String accountNumber { get; set; }
        public String bank { get; set; }
        public String description { get; set; }
        public List<IFormFile> files { get; set; }
    }
}

