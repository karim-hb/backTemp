using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.AccessProfile
{
    public class AccessProfileEditRequest
    {
        public Int32 id { get; set; }
        public String title { get; set; }
   }
}

