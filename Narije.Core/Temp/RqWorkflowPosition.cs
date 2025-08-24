using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqWorkflowPosition
    {
        public int Id { get; set; }
        public int PositionId { get; set; }
        public int WorkflowId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPosition Position { get; set; }
        public virtual RqWorkflow Workflow { get; set; }
    }
}
