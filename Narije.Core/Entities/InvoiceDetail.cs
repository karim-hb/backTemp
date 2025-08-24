using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class InvoiceDetail
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int FoodId { get; set; }
        public int Qty { get; set; }
        public int Price { get; set; }
        public long TotalPrice { get; set; }
        public int Vat { get; set; }
        public long FinalPrice { get; set; }

        public virtual Food Food { get; set; }
        public virtual Invoice Invoice { get; set; }
    }
}
