using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrUser
    {
        public HrUser()
        {
            HrNotificationUsers = new HashSet<HrNotificationUser>();
            HrPeople = new HashSet<HrPerson>();
            HrRoleUsers = new HashSet<HrRoleUser>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RememberToken { get; set; }
        public int? ActiveRole { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<HrNotificationUser> HrNotificationUsers { get; set; }
        public virtual ICollection<HrPerson> HrPeople { get; set; }
        public virtual ICollection<HrRoleUser> HrRoleUsers { get; set; }
    }
}
