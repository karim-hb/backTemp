using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqWorkflowUser
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int WorkflowId { get; set; }

        public virtual HrPerson Person { get; set; }
        public virtual RqWorkflow Workflow { get; set; }
    }
}
