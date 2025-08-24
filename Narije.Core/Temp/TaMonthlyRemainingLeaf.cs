using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaMonthlyRemainingLeaf
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int Year { get; set; }
        public byte Month { get; set; }
        public int Amount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
    }
}
