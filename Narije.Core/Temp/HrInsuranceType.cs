using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrInsuranceType
    {
        public HrInsuranceType()
        {
            HrPersonInsurances = new HashSet<HrPersonInsurance>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? CompanyCode { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<HrPersonInsurance> HrPersonInsurances { get; set; }
    }
}
