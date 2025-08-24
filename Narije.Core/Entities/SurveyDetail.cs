using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class SurveyDetail
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public int SurveyItemId { get; set; }
        public int? SurveyValueId { get; set; }
        public int Value { get; set; }

        public virtual Survey Survey { get; set; }
        public virtual SurveyItem SurveyItem { get; set; }
        public virtual SurveryValue SurveyValue { get; set; }
    }
}
