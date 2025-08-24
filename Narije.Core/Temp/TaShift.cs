using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaShift
    {
        public TaShift()
        {
            TaShiftPeople = new HashSet<TaShiftPerson>();
            TaShiftWorkingHours = new HashSet<TaShiftWorkingHour>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? CalendarId { get; set; }
        public bool? Active { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual TaCalendar Calendar { get; set; }
        public virtual ICollection<TaShiftPerson> TaShiftPeople { get; set; }
        public virtual ICollection<TaShiftWorkingHour> TaShiftWorkingHours { get; set; }
    }
}
