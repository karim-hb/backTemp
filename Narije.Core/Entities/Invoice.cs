using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Invoice
    {
        public Invoice()
        {
            InvoiceDetails = new HashSet<InvoiceDetail>();
        }

        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int CustomerId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public long TotalPrice { get; set; }
        public long FinalPrice { get; set; }
        public int Qty { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool HasVat { get; set; }
        public long Vat { get; set; }
        public string Description { get; set; }
        public string Serial { get; set; }
        public int TransportFee { get; set; }
        public int TransportQty { get; set; }
        public int PayType { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }
    }
}
