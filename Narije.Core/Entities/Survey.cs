using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Survey
    {
        public Survey()
        {
            SurveyDetails = new HashSet<SurveyDetail>();
        }

        public int Id { get; set; }
        public int FoodId { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public int PositiveIndex { get; set; }
        public int NegativeIndex { get; set; }
        public DateTime DateTime { get; set; }
        public int Score { get; set; }
        public int? ReserveId { get; set; }
        public String Description { get; set; }
        public int? GalleryId { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Food Food { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<SurveyDetail> SurveyDetails { get; set; }
    }
}
