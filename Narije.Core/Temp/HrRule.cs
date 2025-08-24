using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrRule
    {
        public HrRule()
        {
            TaPolicyRules = new HashSet<TaPolicyRule>();
        }

        public int Id { get; set; }
        public string Filename { get; set; }
        public int TypeId { get; set; }
        public bool? Visible { get; set; }
        public bool IsPrivate { get; set; }
        public bool? Active { get; set; }
        public bool AddColumn { get; set; }
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual HrType Type { get; set; }
        public virtual ICollection<TaPolicyRule> TaPolicyRules { get; set; }
    }
}
