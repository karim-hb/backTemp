using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrCity
    {
        public HrCity()
        {
            HrCompanies = new HashSet<HrCompany>();
            HrPeople = new HashSet<HrPerson>();
        }

        public int Id { get; set; }
        public int ProvinceId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Name { get; set; }

        public virtual ICollection<HrCompany> HrCompanies { get; set; }
        public virtual ICollection<HrPerson> HrPeople { get; set; }
    }
}
