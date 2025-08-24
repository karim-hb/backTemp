using System;

namespace Narije.Core.DTOs.ViewModels.FoodPrice
{
    public class FoodPriceResponse
    {
        public Int32 id { get; set; }
        public Int32 foodId { get; set; }
        public Int32 groupId { get; set; }
        public Int32? galleryId { get; set; }
        public Int32? customerId { get; set; }
        public Int32 echoPrice { get; set; }
        public Int32 specialPrice { get; set; }
        public Boolean? hasType { get; set; }
        public Boolean isFood { get; set; }
        public String customer { get; set; }
        public String title { get; set; }
   }
}

