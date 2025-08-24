using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.SurveyDetail
{
    public class SurveyDetailEditRequest
    {
        public Int32 id { get; set; }
        public Int32 surveyId { get; set; }
        public Int32 surveyItemId { get; set; }
        public Int32? surveyValueId { get; set; }
        public Int32 value { get; set; }
   }
}

