using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class vCustomerFoodPrice
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public int EchoPrice { get; set; }
        public int SpecialPrice { get; set; }
        public String Title { get; set; }
        public String Description { get; set; }
        public int GroupId { get; set; }
        public String GroupTitle { get; set; }
        public int GalleryId { get; set; }
        public bool IsFood { get; set; }
    }
}
