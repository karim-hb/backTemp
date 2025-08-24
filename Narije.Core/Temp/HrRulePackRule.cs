using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrRulePackRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RuleId { get; set; }
        public int RulePackId { get; set; }
        public string Values { get; set; }
        public bool? Active { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual HrRulePack RulePack { get; set; }
    }
}
