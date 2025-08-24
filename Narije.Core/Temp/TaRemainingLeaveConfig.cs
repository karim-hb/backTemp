using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaRemainingLeaveConfig
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int Year { get; set; }
        public bool IsLocked { get; set; }
        public bool Transferable { get; set; }
        public bool Usable { get; set; }
        public bool RejectStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
    }
}
