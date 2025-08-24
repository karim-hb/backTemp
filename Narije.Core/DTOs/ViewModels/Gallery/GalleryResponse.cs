using System;

namespace Narije.Core.DTOs.ViewModels.Gallery
{
    public class GalleryResponse
    {
        public Int32 id { get; set; }
        public String originalFileName { get; set; }
        public String systemFileName { get; set; }
        public String source { get; set; }
        public String alt { get; set; }
   }
}

