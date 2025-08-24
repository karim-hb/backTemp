using Narije.Core.DTOs.ViewModels.SurveyDetail;
using System;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Survey
{
    public class SurveyResponse
    {
        public Int32 id { get; set; }
        public Int32 foodId { get; set; }
        public Int32 userId { get; set; }
        public Int32 customerId { get; set; }
        public Int32 positiveIndex { get; set; }
        public Int32 negativeIndex { get; set; }
        public DateTime dateTime { get; set; }
        public DateTime reserveTime { get; set; }
        public Int32 score { get; set; }
        public Int32? reserveId { get; set; }
        public String customer { get; set; }
        public String food { get; set; }
        public String user { get; set; }
        public String description { get; set; }
        public List<SurveyDetailResponse> positive { get; set; }
        public List<SurveyDetailResponse> negative { get; set; }
        public String pos { get; set; }
        public String neg { get; set; }
        public int? galleryId { get; set; }
    }
}

