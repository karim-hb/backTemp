using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaDevice
    {
        public TaDevice()
        {
            TaClockings = new HashSet<TaClocking>();
            TaDeviceClockingMaps = new HashSet<TaDeviceClockingMap>();
            TaDeviceEnterTypes = new HashSet<TaDeviceEnterType>();
            TaDevicePositions = new HashSet<TaDevicePosition>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int? RefreshCycle { get; set; }
        public bool? Active { get; set; }
        public int TypeId { get; set; }
        public bool? SaveDraft { get; set; }
        public string Description { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<TaClocking> TaClockings { get; set; }
        public virtual ICollection<TaDeviceClockingMap> TaDeviceClockingMaps { get; set; }
        public virtual ICollection<TaDeviceEnterType> TaDeviceEnterTypes { get; set; }
        public virtual ICollection<TaDevicePosition> TaDevicePositions { get; set; }
    }
}
