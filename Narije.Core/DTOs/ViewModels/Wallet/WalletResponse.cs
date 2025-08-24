using System;

namespace Narije.Core.DTOs.ViewModels.Wallet
{
    public class WalletResponse
    {
        public Int32 id { get; set; }
        public Int32 userId { get; set; }
        public Int32? customerId { get; set; }
        public DateTime dateTime { get; set; }
        public Int64 preValue { get; set; }
        public Int32 op { get; set; }
        public Int64 value { get; set; }
        public Int64 remValue { get; set; }
        public String user { get; set; }
        public DateTime? lastCredit { get; set; }
        public String userMobile { get; set; }
        public int? orderId { get; set; }
        public int? loanRequestId { get; set; }
        public String refNumber { get; set; }
        public String pan { get; set; }
        public Int32 gateway { get; set; }
        public String fName { get; set; }
        public String lName { get; set; }
        public String description { get; set; }
        public int? lastCreditId { get; set; }
    }
}

