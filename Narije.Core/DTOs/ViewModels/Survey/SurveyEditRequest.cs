using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Survey
{
    public class SurveyEditRequest
    {
        public Int32 id { get; set; }
        public Int32 foodId { get; set; }
        public Int32 userId { get; set; }
        public Int32 customerId { get; set; }
        public Int32 positiveIndex { get; set; }
        public Int32 negativeIndex { get; set; }
        public DateTime dateTime { get; set; }
        public Int32 score { get; set; }
        public Int32? reserveId { get; set; }
   }
}

