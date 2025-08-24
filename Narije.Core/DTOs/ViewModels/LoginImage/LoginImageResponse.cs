using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.LoginImage
{
    public class LoginImageResponse
    {
        public int id { get; set; }
        public string title { get; set; }
        public int? galleryId { get; set; }
        public string description { get; set; }
        public bool forMobile { get; set; }
    }
}
