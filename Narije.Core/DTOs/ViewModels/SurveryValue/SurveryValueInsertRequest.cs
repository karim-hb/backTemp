using System;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.SurveryValue
{
    public class SurveryValueInsertRequest
    {
        public String title { get; set; }
        public Int32 value { get; set; }
        public Boolean active { get; set; }
        public Int32 itemId { get; set; }
   }
}

