using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class AccessPermission
    {
        public int Id { get; set; }
        public int AccessId { get; set; }
        public int PermissionId { get; set; }

        public virtual AccessProfile Access { get; set; }
        public virtual Permission Permission { get; set; }
    }
}
