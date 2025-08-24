using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Permission
    {
        public Permission()
        {
            Childrens = new List<Permission>();
            AccessPermissions = new HashSet<AccessPermission>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public int Module { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public int Priority { get; set; }
        public int? ParentId { get; set; }
        public bool Active { get; set; }
        public virtual ICollection<AccessPermission> AccessPermissions { get; set; }
        public virtual Permission Parent { get; set; }
        public virtual List<Permission> Childrens { get; set; }
    }
}
