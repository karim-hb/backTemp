using System;

namespace Narije.Core.DTOs.ViewModels.AccessPermission
{
    public class AccessPermissionResponse
    {
        public Int32 id { get; set; }
        public Int32 accessId { get; set; }
        public Int32 permissionId { get; set; }
        public String access { get; set; }
        public String permission { get; set; }
   }
}

