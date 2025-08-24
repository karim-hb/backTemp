using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaFactor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RuleId { get; set; }
        public int? FactorId { get; set; }
        public int WorkingHourId { get; set; }
        public string Values { get; set; }
        public byte Priority { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual TaWorkingHour WorkingHour { get; set; }
    }
}
