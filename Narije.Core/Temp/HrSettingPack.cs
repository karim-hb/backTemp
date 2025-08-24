using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrSettingPack
    {
        public HrSettingPack()
        {
            HrSettings = new HashSet<HrSetting>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public string Color { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<HrSetting> HrSettings { get; set; }
    }
}
