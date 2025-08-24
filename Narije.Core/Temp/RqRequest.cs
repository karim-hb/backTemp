using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqRequest
    {
        public RqRequest()
        {
            RqReceivedRequests = new HashSet<RqReceivedRequest>();
            RqRequestables = new HashSet<RqRequestable>();
        }

        public int Id { get; set; }
        public int PositionId { get; set; }
        public int PersonId { get; set; }
        public int? SubstituteId { get; set; }
        public int TypeId { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool Done { get; set; }
        public bool? Active { get; set; }
        public byte Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Sender { get; set; }
        public string Values { get; set; }
        public string Props { get; set; }

        public virtual HrPerson Person { get; set; }
        public virtual HrPosition Position { get; set; }
        public virtual HrType Type { get; set; }
        public virtual ICollection<RqReceivedRequest> RqReceivedRequests { get; set; }
        public virtual ICollection<RqRequestable> RqRequestables { get; set; }
    }
}
