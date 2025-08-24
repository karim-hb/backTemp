using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.InvoiceDetail
{
    public class InvoiceDetailEditRequest
    {
        public Int32 id { get; set; }
        public Int32 invoiceId { get; set; }
        public Int32 foodId { get; set; }
        public Int32 qty { get; set; }
        public Int32 price { get; set; }
        public Int64 totalPrice { get; set; }
        public Int32 vat { get; set; }
        public Int64 finalPrice { get; set; }
   }
}

