using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.SurveyItem
{
    public class SurveyItemEditRequest
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public Int32 itemType { get; set; }
        public Boolean active { get; set; }
        public String value { get; set; }
        public bool? hasSeparateItems { get; set; }
    }
}

