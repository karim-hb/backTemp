using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPosition
    {
        public HrPosition()
        {
            FeedingReserfPositionIdEatenNavigations = new HashSet<FeedingReserf>();
            FeedingReserfPositionIdOrderedNavigations = new HashSet<FeedingReserf>();
            RqReceivedRequests = new HashSet<RqReceivedRequest>();
            RqRequestRulePackPositions = new HashSet<RqRequestRulePackPosition>();
            RqRequests = new HashSet<RqRequest>();
            RqWorkflowApprovers = new HashSet<RqWorkflowApprover>();
            RqWorkflowPositions = new HashSet<RqWorkflowPosition>();
            TaPolicyPositions = new HashSet<TaPolicyPosition>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? PositionId { get; set; }
        public byte Type { get; set; }
        public int? PersonId { get; set; }
        public bool? Active { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
        public virtual ICollection<FeedingReserf> FeedingReserfPositionIdEatenNavigations { get; set; }
        public virtual ICollection<FeedingReserf> FeedingReserfPositionIdOrderedNavigations { get; set; }
        public virtual ICollection<RqReceivedRequest> RqReceivedRequests { get; set; }
        public virtual ICollection<RqRequestRulePackPosition> RqRequestRulePackPositions { get; set; }
        public virtual ICollection<RqRequest> RqRequests { get; set; }
        public virtual ICollection<RqWorkflowApprover> RqWorkflowApprovers { get; set; }
        public virtual ICollection<RqWorkflowPosition> RqWorkflowPositions { get; set; }
        public virtual ICollection<TaPolicyPosition> TaPolicyPositions { get; set; }
    }
}
