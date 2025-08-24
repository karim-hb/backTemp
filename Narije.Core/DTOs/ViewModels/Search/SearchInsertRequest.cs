using System;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Search
{
    public class SearchInsertRequest
    {
        public String tableName { get; set; }
        public String fieldName { get; set; }
        public String fieldType { get; set; }
   }
}

