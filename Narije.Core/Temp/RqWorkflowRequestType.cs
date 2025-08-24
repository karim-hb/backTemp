using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqWorkflowRequestType
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int WorkflowId { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrType Type { get; set; }
        public virtual RqWorkflow Workflow { get; set; }
    }
}
