using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class SurveryValue
    {
        public SurveryValue()
        {
            SurveyDetails = new HashSet<SurveyDetail>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int Value { get; set; }
        public bool Active { get; set; }
        public int ItemId { get; set; }

        public virtual SurveyItem Item { get; set; }
        public virtual ICollection<SurveyDetail> SurveyDetails { get; set; }
    }
}
