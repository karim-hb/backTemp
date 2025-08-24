using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Search
{
    public class SearchEditRequest
    {
        public Int32 id { get; set; }
        public String tableName { get; set; }
        public String fieldName { get; set; }
        public String fieldType { get; set; }
   }
}

