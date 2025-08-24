using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class ChangePasswordRequest
    {
        public int id { get; set; }
        public string password { get; set; }
    }
}
