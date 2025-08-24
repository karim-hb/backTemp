using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class SurveyItem
    {
        public SurveyItem()
        {
            SurveryValues = new HashSet<SurveryValue>();
            SurveyDetails = new HashSet<SurveyDetail>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int ItemType { get; set; }
        public bool Active { get; set; }
        public string Value { get; set; }
        public bool HasSeparateItems { get; set; }

        public virtual ICollection<SurveryValue> SurveryValues { get; set; }
        public virtual ICollection<SurveyDetail> SurveyDetails { get; set; }
    }
}
