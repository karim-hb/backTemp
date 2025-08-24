using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaRemainingLeave
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int Year { get; set; }
        public int RemainingLeaves { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
    }
}
