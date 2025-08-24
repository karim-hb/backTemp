using System;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.City
{
    public class CityInsertRequest
    {
        public String title { get; set; }
        public String code { get; set; }
        public Int32 provinceId { get; set; }
        public Int32 transportFee { get; set; }
   }
}

