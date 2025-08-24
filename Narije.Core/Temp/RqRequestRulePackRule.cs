using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqRequestRulePackRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeId { get; set; }
        public int RequestRulePackId { get; set; }
        public string Values { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual RqRequestRulePack RequestRulePack { get; set; }
        public virtual HrType Type { get; set; }
    }
}
