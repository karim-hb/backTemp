using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrRoleUser
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual HrRole Role { get; set; }
        public virtual HrUser User { get; set; }
    }
}
