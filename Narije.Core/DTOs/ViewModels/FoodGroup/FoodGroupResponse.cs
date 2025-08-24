using System;

namespace Narije.Core.DTOs.ViewModels.FoodGroup
{
    public class FoodGroupResponse
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public Int32? galleryId { get; set; }
        public Boolean invoiceAddOn { get; set; }
        public int totalFood { get; set; }
        public string arpaNumber { get; set; }
        public string description { get; set; }
    }
}

