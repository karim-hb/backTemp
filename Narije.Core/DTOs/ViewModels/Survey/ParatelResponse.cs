using Narije.Core.DTOs.ViewModels.SurveyDetail;
using System;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Survey
{
    public class ParatelResponse
    {
        public Int32 negativeItem { get; set; }
        public Int32 value { get; set; }
        public Int32 cf { get; set; }
        public double cfp { get; set; }
        public String title { get; set; }
    }
}

