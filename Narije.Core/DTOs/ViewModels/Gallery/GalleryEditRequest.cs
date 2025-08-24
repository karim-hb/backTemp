using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Gallery
{
    public class GalleryEditRequest
    {
        public Int32 id { get; set; }
        public String originalFileName { get; set; }
        public String source { get; set; }
        public String alt { get; set; }
    }
}

