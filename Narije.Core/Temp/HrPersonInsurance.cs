using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPersonInsurance
    {
        public HrPersonInsurance()
        {
            HrPersonContracts = new HashSet<HrPersonContract>();
        }

        public int Id { get; set; }
        public int TypeId { get; set; }
        public string Code { get; set; }
        public int PersonId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
        public virtual HrInsuranceType Type { get; set; }
        public virtual ICollection<HrPersonContract> HrPersonContracts { get; set; }
    }
}
