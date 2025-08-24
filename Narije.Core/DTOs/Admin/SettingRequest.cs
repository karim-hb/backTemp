using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class SettingRequest
    {
        public string companyName { get; set; }
        public string address { get; set; }
        public string tel { get; set; }
        public string economicCode { get; set; }
        public string nationalId { get; set; }
        public string regNumber { get; set; }
        public string contactMobile { get; set; }
        public string postalCode { get; set; }
        public int? cityId { get; set; }
        public int? provinceId { get; set; }
        public string fromGallery { get; set; }
        public List<IFormFile> files { get; set; }

        public string fromGalleryDarkLogo { get; set; }
        public List<IFormFile> darkLogoFiles { get; set; }
    }
}
