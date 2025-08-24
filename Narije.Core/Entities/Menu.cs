using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Menu
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime DateTime { get; set; }
        public int FoodId { get; set; }
        public int MaxReserve { get; set; }
        public int FoodType { get; set; }
        public int? MealType { get; set; }
        public int? EchoPrice { get; set; }
        public int? SpecialPrice { get; set; }
        public int? MenuInfoId { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Food Food { get; set; }
    }
}
