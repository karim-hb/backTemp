using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrRecruitmentType
    {
        public HrRecruitmentType()
        {
            HrPersonContracts = new HashSet<HrPersonContract>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? CompanyCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<HrPersonContract> HrPersonContracts { get; set; }
    }
}
