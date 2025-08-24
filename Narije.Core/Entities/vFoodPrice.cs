using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class vFoodPrice
    {
        public int? Id { get; set; }
        public int FoodId { get; set; }
        public bool IsFood { get; set; }
        public int GroupId { get; set; }
        public int? GalleryId { get; set; }
        public String Title { get; set; }
        public int? CustomerId { get; set; }
        public int? EchoPrice { get; set; }
        public int? SpecialPrice { get; set; }
        public bool HasType { get; set; }

    }
}
