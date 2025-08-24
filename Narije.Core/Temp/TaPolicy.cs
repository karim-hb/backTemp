using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaPolicy
    {
        public TaPolicy()
        {
            TaPolicyPositions = new HashSet<TaPolicyPosition>();
            TaPolicyRules = new HashSet<TaPolicyRule>();
            TaPolicyUsers = new HashSet<TaPolicyUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<TaPolicyPosition> TaPolicyPositions { get; set; }
        public virtual ICollection<TaPolicyRule> TaPolicyRules { get; set; }
        public virtual ICollection<TaPolicyUser> TaPolicyUsers { get; set; }
    }
}
