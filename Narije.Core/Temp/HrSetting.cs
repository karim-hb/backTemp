using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrSetting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeId { get; set; }
        public int SettingPackId { get; set; }
        public string Values { get; set; }
        public bool? Active { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual HrSettingPack SettingPack { get; set; }
        public virtual HrType Type { get; set; }
    }
}
