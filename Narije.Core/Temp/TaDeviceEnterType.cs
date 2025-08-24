using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaDeviceEnterType
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int DeviceId { get; set; }
        public int DeviceBioTypeId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual TaDevice Device { get; set; }
    }
}
