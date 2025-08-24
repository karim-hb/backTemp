using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class AccessProfile
    {
        public AccessProfile()
        {
            AccessPermissions = new HashSet<AccessPermission>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<AccessPermission> AccessPermissions { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
