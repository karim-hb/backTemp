using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPermission
    {
        public HrPermission()
        {
            HrPermissionRoles = new HashSet<HrPermissionRole>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ModuleStr { get; set; }
        public int? ModuleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual HrRuleSubModule Module { get; set; }
        public virtual ICollection<HrPermissionRole> HrPermissionRoles { get; set; }
    }
}
