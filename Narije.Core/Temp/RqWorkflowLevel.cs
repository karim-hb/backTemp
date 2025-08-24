using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqWorkflowLevel
    {
        public RqWorkflowLevel()
        {
            RqWorkflowApprovers = new HashSet<RqWorkflowApprover>();
        }

        public int Id { get; set; }
        public int Priority { get; set; }
        public int WorkflowId { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual RqWorkflow Workflow { get; set; }
        public virtual ICollection<RqWorkflowApprover> RqWorkflowApprovers { get; set; }
    }
}
