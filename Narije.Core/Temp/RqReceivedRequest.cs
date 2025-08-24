using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqReceivedRequest
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int? ApproverId { get; set; }
        public int? LevelId { get; set; }
        public bool? Active { get; set; }
        public byte Status { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPosition Approver { get; set; }
        public virtual RqRequest Request { get; set; }
    }
}
