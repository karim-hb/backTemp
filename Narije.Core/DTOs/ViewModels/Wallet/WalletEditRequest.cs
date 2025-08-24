using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Wallet
{
    public class WalletEditRequest
    {
        public Int32 id { get; set; }
        public Int32 userId { get; set; }
        public DateTime dateTime { get; set; }
        public Int64 preValue { get; set; }
        public Int32 op { get; set; }
        public Int64 value { get; set; }
        public Int64 remValue { get; set; }
        public String opkey { get; set; }
   }
}

