using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class FoodRequest
    {
        public int? id { get; set; }
        public string title { get; set; }
        public int groupId { get; set; }
        public bool active { get; set; }
        public bool isDaily { get; set; }
        public bool isGuest { get; set; }
        public bool hasType { get; set; }
        public int echoPrice { get; set; }
        public int specialPrice { get; set; }
        public int? vat { get; set; }
        public string fromGallery { get; set; }
        public List<IFormFile> files { get; set; }
    }
}
