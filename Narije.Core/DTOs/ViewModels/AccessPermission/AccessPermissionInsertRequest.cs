using System;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.AccessPermission
{
    public class AccessPermissionInsertRequest
    {
        public Int32 accessId { get; set; }
        public Int32 permissionId { get; set; }
   }
}

