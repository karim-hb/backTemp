using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Food
{
    public class FoodEditRequest
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public String description { get; set; }
        public Int32 groupId { get; set; }
        public Boolean active { get; set; }
        public string fromGallery { get; set; }
        public List<IFormFile> files { get; set; }
        public Boolean? isDaily { get; set; }
        public Boolean? hasType { get; set; }
        public Boolean isGuest { get; set; }
        public Int32 echoPrice { get; set; }
        public Int32 specialPrice { get; set; }
        public Int32? vat { get; set; } 
        public Int32? productType { get; set; }
        public string arpaNumber { get; set; }
        public Boolean? vip { get; set; }
        public Boolean isFood { get; set; }
        public object imageData { get; set; }

    }
}

