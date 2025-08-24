using System;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.SurveyDetail
{
    public class SurveyDetailInsertRequest
    {
        public Int32 surveyItemId { get; set; }
        public Int32? surveyValueId { get; set; }
        public Int32 value { get; set; }
   }
}

