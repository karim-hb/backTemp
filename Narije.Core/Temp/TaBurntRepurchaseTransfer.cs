using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaBurntRepurchaseTransfer
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int Year { get; set; }
        public int Remaining { get; set; }
        public byte Type { get; set; }
        public bool RejectStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
    }
}
