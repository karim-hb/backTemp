using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrRole
    {
        public HrRole()
        {
            HrPermissionRoles = new HashSet<HrPermissionRole>();
            HrRoleUsers = new HashSet<HrRoleUser>();
            Scopes = new HashSet<Scope>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public byte Scope { get; set; }

        public virtual ICollection<HrPermissionRole> HrPermissionRoles { get; set; }
        public virtual ICollection<HrRoleUser> HrRoleUsers { get; set; }
        public virtual ICollection<Scope> Scopes { get; set; }
    }
}
