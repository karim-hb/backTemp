using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Survey
{
    public class SurveyInsertRequest
    {
        public Int32 score { get; set; }
        public Int32 reserveId { get; set; }
        public String positive { get; set; }
        public String negative { get; set; }
        public String description { get; set; }
        public List<IFormFile> files { get; set; }

    }
}

