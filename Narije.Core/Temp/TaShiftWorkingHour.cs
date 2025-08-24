using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaShiftWorkingHour
    {
        public int Id { get; set; }
        public int ShiftId { get; set; }
        public DateTime Date { get; set; }
        public int? WorkingHoursId { get; set; }
        public string WorkingHoursName { get; set; }
        public int? PoliciesId { get; set; }
        public string PoliciesName { get; set; }
        public int? SettingsId { get; set; }
        public string SettingsName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual TaShift Shift { get; set; }
        public virtual TaWorkingHour WorkingHours { get; set; }
    }
}
