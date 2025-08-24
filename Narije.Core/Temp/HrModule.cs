using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrModule
    {
        public HrModule()
        {
            HrTypes = new HashSet<HrType>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? ModuleId { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<HrType> HrTypes { get; set; }
    }
}
