using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaShiftPerson
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int ShiftId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
        public virtual TaShift Shift { get; set; }
    }
}
