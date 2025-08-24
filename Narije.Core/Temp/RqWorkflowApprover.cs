using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqWorkflowApprover
    {
        public int Id { get; set; }
        public int? ApproverId { get; set; }
        public int LevelId { get; set; }
        public string PositionName { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPosition Approver { get; set; }
        public virtual RqWorkflowLevel Level { get; set; }
    }
}
