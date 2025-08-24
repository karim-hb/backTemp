using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaCalendarDay
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CalendarDayTypeId { get; set; }
        public int CalendarId { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual TaCalendar Calendar { get; set; }
        public virtual TaCalendarDayType CalendarDayType { get; set; }
    }
}
