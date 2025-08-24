using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Permission
{
    public class PermissionEditRequest
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public String value { get; set; }
        public Int32 module { get; set; }
        public String method { get; set; }
        public String url { get; set; }
        public Int32? parentId { get; set; }
   }
}

