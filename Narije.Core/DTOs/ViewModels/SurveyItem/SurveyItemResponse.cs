using Narije.Core.DTOs.ViewModels.SurveryValue;
using System;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.SurveyItem
{
    public class SurveyItemResponse
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public Int32 itemType { get; set; }
        public Boolean active { get; set; }
        public Boolean hasSeparateItems { get; set; }
        public String value { get; set; }
        public List<SurveryValueResponse> values { get; set; }
    }
}

