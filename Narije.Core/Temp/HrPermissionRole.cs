using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPermissionRole
    {
        public int Id { get; set; }
        public int PermissionId { get; set; }
        public int RoleId { get; set; }

        public virtual HrPermission Permission { get; set; }
        public virtual HrRole Role { get; set; }
    }
}
