using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPersonContract
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int RecruitmentTypeId { get; set; }
        public int? InsuranceId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPersonInsurance Insurance { get; set; }
        public virtual HrPerson Person { get; set; }
        public virtual HrRecruitmentType RecruitmentType { get; set; }
    }
}
