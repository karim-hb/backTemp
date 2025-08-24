using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrType
    {
        public HrType()
        {
            HrForms = new HashSet<HrForm>();
            HrNotifications = new HashSet<HrNotification>();
            HrRules = new HashSet<HrRule>();
            HrSettings = new HashSet<HrSetting>();
            RqRequestRulePackRules = new HashSet<RqRequestRulePackRule>();
            RqRequests = new HashSet<RqRequest>();
            RqWorkflowRequestTypes = new HashSet<RqWorkflowRequestType>();
            TaClockings = new HashSet<TaClocking>();
            TaWrits = new HashSet<TaWrit>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public int? TypeId { get; set; }
        public int ModuleId { get; set; }
        public int? ForModuleId { get; set; }
        public int? CompanyCode { get; set; }
        public string FullName { get; set; }
        public string FullLabel { get; set; }

        public virtual HrModule Module { get; set; }
        public virtual ICollection<HrForm> HrForms { get; set; }
        public virtual ICollection<HrNotification> HrNotifications { get; set; }
        public virtual ICollection<HrRule> HrRules { get; set; }
        public virtual ICollection<HrSetting> HrSettings { get; set; }
        public virtual ICollection<RqRequestRulePackRule> RqRequestRulePackRules { get; set; }
        public virtual ICollection<RqRequest> RqRequests { get; set; }
        public virtual ICollection<RqWorkflowRequestType> RqWorkflowRequestTypes { get; set; }
        public virtual ICollection<TaClocking> TaClockings { get; set; }
        public virtual ICollection<TaWrit> TaWrits { get; set; }
    }
}
