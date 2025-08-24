using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.LoginImage
{
    public class LoginImageEditRequest
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public bool forMobile { get; set; }
        public string fromGallery { get; set; }
        public List<IFormFile> files { get; set; }
    }
}
