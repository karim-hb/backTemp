using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.FoodGroup
{
    public class FoodGroupEditRequest
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public string fromGallery { get; set; }
        public List<IFormFile> files { get; set; }
        public Boolean? invoiceAddOn { get; set; }
        public string arpaNumber { get; set; }
        public string description { get; set; }
    }
}

