using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaCalendar
    {
        public TaCalendar()
        {
            TaCalendarDays = new HashSet<TaCalendarDay>();
            TaShifts = new HashSet<TaShift>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<TaCalendarDay> TaCalendarDays { get; set; }
        public virtual ICollection<TaShift> TaShifts { get; set; }
    }
}
