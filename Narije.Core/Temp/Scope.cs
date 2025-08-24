using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class Scope
    {
        public int Id { get; set; }
        public int? RoleId { get; set; }
        public string ScopeModel { get; set; }
        public string Cases { get; set; }
        public int Self { get; set; }
        public int All { get; set; }
        public int Children { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual HrRole Role { get; set; }
    }
}
