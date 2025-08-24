using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqRequestRulePack
    {
        public RqRequestRulePack()
        {
            RqRequestRulePackPositions = new HashSet<RqRequestRulePackPosition>();
            RqRequestRulePackRules = new HashSet<RqRequestRulePackRule>();
            RqRequestRulePackUsers = new HashSet<RqRequestRulePackUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<RqRequestRulePackPosition> RqRequestRulePackPositions { get; set; }
        public virtual ICollection<RqRequestRulePackRule> RqRequestRulePackRules { get; set; }
        public virtual ICollection<RqRequestRulePackUser> RqRequestRulePackUsers { get; set; }
    }
}
