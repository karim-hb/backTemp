using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Province
{
    public class ProvinceEditRequest
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public String code { get; set; }
   }
}

