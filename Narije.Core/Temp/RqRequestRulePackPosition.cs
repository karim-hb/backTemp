using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqRequestRulePackPosition
    {
        public int Id { get; set; }
        public int RequestRulePackId { get; set; }
        public int PositionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPosition Position { get; set; }
        public virtual RqRequestRulePack RequestRulePack { get; set; }
    }
}
