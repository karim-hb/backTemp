using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqWorkflow
    {
        public RqWorkflow()
        {
            RqWorkflowLevels = new HashSet<RqWorkflowLevel>();
            RqWorkflowPositions = new HashSet<RqWorkflowPosition>();
            RqWorkflowRequestTypes = new HashSet<RqWorkflowRequestType>();
            RqWorkflowUsers = new HashSet<RqWorkflowUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public bool Final { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<RqWorkflowLevel> RqWorkflowLevels { get; set; }
        public virtual ICollection<RqWorkflowPosition> RqWorkflowPositions { get; set; }
        public virtual ICollection<RqWorkflowRequestType> RqWorkflowRequestTypes { get; set; }
        public virtual ICollection<RqWorkflowUser> RqWorkflowUsers { get; set; }
    }
}
