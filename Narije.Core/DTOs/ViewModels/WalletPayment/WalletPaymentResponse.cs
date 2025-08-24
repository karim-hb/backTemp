using System;

namespace Narije.Core.DTOs.ViewModels.WalletPayment
{
    public class WalletPaymentResponse
    {
        public Int32 id { get; set; }
        public Int32 op { get; set; }
        public Int32 userId { get; set; }
        public Int32? applierId { get; set; }
        public Int32? applicantId { get; set; }
        public Int64 value { get; set; }
        public Int32? walletId { get; set; }
        public DateTime dateTime { get; set; }
        public Int32 status { get; set; }
        public Int32 customerId { get; set; }
        public Int32 gateway { get; set; }
        public Int32? galleryId { get; set; }
        public String refNumber { get; set; }
        public String result { get; set; }
        public String pan { get; set; }
        public DateTime? updatedAt { get; set; }
        public String fName { get; set; }
        public String lName { get; set; }
        public String user { get; set; }
        public String applier { get; set; }
        public String applicant { get; set; }
        public String bank { get; set; }
        public String description { get; set; }
        public String accountNumber { get; set; }
        public String userMobile { get; set; }
    }
}

