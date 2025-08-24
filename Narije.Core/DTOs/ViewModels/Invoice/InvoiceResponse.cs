using System;

namespace Narije.Core.DTOs.ViewModels.Invoice
{
    public class InvoiceResponse
    {
        public Int32 id { get; set; }
        public DateTime dateTime { get; set; }
        public Int32 customerId { get; set; }
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public Int64 totalPrice { get; set; }
        public Int32 qty { get; set; }
        public DateTime updatedAt { get; set; }
        public Boolean hasVat { get; set; }
        public Int64 vat { get; set; }
        public String description { get; set; }
        public String serial { get; set; }
        public Int64 finalPrice { get; set; }
        public Int32 transportFee { get; set; }
        public Int32 payType { get; set; }
        public Int32 transportQty { get; set; }
        public String customer { get; set; }
   }
}

