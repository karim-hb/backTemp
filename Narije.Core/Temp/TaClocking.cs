using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaClocking
    {
        public int Id { get; set; }
        public int? DeviceId { get; set; }
        public int PersonId { get; set; }
        public string IoId { get; set; }
        public int TypeId { get; set; }
        public DateTime Datetime { get; set; }
        public bool Changed { get; set; }
        public byte Status { get; set; }
        public string Description { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public byte EntryType { get; set; }

        public virtual TaDevice Device { get; set; }
        public virtual HrPerson Person { get; set; }
        public virtual HrType Type { get; set; }
    }
}
