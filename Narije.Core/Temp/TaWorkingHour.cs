using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaWorkingHour
    {
        public TaWorkingHour()
        {
            TaFactors = new HashSet<TaFactor>();
            TaShiftMasks = new HashSet<TaShiftMask>();
            TaShiftWorkingHours = new HashSet<TaShiftWorkingHour>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public TimeSpan TimeFrom { get; set; }
        public int Period { get; set; }
        public int? Duration { get; set; }
        public TimeSpan BeforeStart { get; set; }
        public TimeSpan AfterEnd { get; set; }
        public string Color { get; set; }
        public bool Active { get; set; }
        public string Description { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int DayTo { get; set; }
        public TimeSpan TimeTo { get; set; }

        public virtual ICollection<TaFactor> TaFactors { get; set; }
        public virtual ICollection<TaShiftMask> TaShiftMasks { get; set; }
        public virtual ICollection<TaShiftWorkingHour> TaShiftWorkingHours { get; set; }
    }
}
