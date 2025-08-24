using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaDeviceClockingMap
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public int? ClockingTypeId { get; set; }
        public int ClockingReasonId { get; set; }
        public int? DeviceClockingTypeId { get; set; }
        public int? EntryType { get; set; }

        public virtual TaDevice Device { get; set; }
    }
}
