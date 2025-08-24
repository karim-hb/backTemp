using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrRulePack
    {
        public HrRulePack()
        {
            HrRulePackRules = new HashSet<HrRulePackRule>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public string Color { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<HrRulePackRule> HrRulePackRules { get; set; }
    }
}
