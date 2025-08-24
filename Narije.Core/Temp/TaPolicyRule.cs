using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaPolicyRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RuleId { get; set; }
        public int PolicyId { get; set; }
        public string Values { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual TaPolicy Policy { get; set; }
        public virtual HrRule Rule { get; set; }
    }
}
