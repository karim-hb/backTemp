using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class InvoiceRequest
    {
        public int? id { get; set; }
        public int customerId { get; set; }
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public bool hasVat { get; set; }
        public string description { get; set; }
        public string serial { get; set; }
        public int payType { get; set; }
        public int transportFee { get; set; }
        public int transportQty { get; set; }
        public List<InvoiceDetailRequest> details { get; set; }

    }
}
