using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaCalendarDayType
    {
        public TaCalendarDayType()
        {
            TaCalendarDays = new HashSet<TaCalendarDay>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Color { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<TaCalendarDay> TaCalendarDays { get; set; }
    }
}
