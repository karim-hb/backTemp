using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrProvince
    {
        public HrProvince()
        {
            HrPeople = new HashSet<HrPerson>();
        }

        public int Id { get; set; }
        public int CountryId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Name { get; set; }

        public virtual ICollection<HrPerson> HrPeople { get; set; }
    }
}
