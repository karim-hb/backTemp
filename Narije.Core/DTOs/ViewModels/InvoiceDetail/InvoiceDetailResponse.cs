using System;

namespace Narije.Core.DTOs.ViewModels.InvoiceDetail
{
    public class InvoiceDetailResponse
    {
        public Int32 id { get; set; }
        public Int32 invoiceId { get; set; }
        public Int32 foodId { get; set; }
        public Int32 qty { get; set; }
        public Int32 price { get; set; }
        public Int64 totalPrice { get; set; }
        public Int32 vat { get; set; }
        public Int64 finalPrice { get; set; }
        public String food { get; set; }
        public String invoice { get; set; }
   }
}

