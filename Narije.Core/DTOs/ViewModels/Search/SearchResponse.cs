using System;

namespace Narije.Core.DTOs.ViewModels.Search
{
    public class SearchResponse
    {
        public Int32 id { get; set; }
        public String tableName { get; set; }
        public String fieldName { get; set; }
        public String fieldType { get; set; }
   }
}

