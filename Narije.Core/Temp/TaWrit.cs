using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaWrit
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int? PositionId { get; set; }
        public string Key { get; set; }
        public DateTime RegistrationDatetime { get; set; }
        public int TypeId { get; set; }
        public byte Status { get; set; }
        public string Values { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual HrPerson Person { get; set; }
        public virtual HrType Type { get; set; }
    }
}
