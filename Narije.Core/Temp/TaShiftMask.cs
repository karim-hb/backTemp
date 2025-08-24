using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaShiftMask
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int WorkingHourId { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
        public virtual TaWorkingHour WorkingHour { get; set; }
    }
}
