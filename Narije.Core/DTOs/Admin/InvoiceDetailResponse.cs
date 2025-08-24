using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class InvoiceDetailResponse
    {
        public int row { get; set; }
        public string datetime { get; set; }
        public string food { get; set; }
        public string foodId { get; set; }
        public int qty { get; set; }
        public int price { get; set; }
        public long totalPrice { get; set; }
        public long vat { get; set; }
        public long finalPrice { get; set; }
        public int type { get; set; }

    }
}
