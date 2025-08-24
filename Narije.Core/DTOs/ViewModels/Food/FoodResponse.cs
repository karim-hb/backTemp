using System;

namespace Narije.Core.DTOs.ViewModels.Food
{
    public class FoodResponse
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public String description { get; set; }
        public Int32 groupId { get; set; }
        public Boolean active { get; set; }
        public Int32? galleryId { get; set; }
        public Boolean isDaily { get; set; }
        public Boolean? hasType { get; set; }
        public Boolean isGuest { get; set; }
        public Boolean isFood { get; set; }
        public Int32 echoPrice { get; set; }
        public Int32 specialPrice { get; set; }
        public Int32? vat { get; set; }
        public Int32? productType { get; set; }
        public string arpaNumber { get; set; }
        public Boolean? vip { get; set; }
    }
}

