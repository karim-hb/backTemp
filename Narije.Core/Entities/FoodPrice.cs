using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class FoodPrice
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public int CustomerId { get; set; }
        public int EchoPrice { get; set; }
        public int SpecialPrice { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Food Food { get; set; }
    }
}
