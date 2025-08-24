using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaDevicePosition
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DeviceId { get; set; }
        public int? ParentId { get; set; }
        public byte Type { get; set; }
        public int ProvinceId { get; set; }
        public int CityId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual TaDevice Device { get; set; }
    }
}
