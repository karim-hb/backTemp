using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class FoodGroupRequest
    {
        public int? id { get; set; }
        public bool isFood { get; set; }
        public string title { get; set; }
        public string fromGallery { get; set; }
        public List<IFormFile> files { get; set; }
    }
}
