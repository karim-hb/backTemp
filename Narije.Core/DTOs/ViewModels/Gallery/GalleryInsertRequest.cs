using System;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Gallery
{
    public class GalleryInsertRequest
    {
        public List<IFormFile> files { get; set; }

    }
}

