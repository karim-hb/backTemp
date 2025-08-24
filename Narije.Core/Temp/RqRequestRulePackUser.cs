using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqRequestRulePackUser
    {
        public int Id { get; set; }
        public int RequestRulePackId { get; set; }
        public int PersonId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrPerson Person { get; set; }
        public virtual RqRequestRulePack RequestRulePack { get; set; }
    }
}
