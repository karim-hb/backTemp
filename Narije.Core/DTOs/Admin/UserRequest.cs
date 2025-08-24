using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class UserRequest
    {
        public int? id { get; set; }
        public string fName { get; set; }
        public string lName { get; set; }
        public string mobile { get; set; }
        public string description { get; set; }
        public int? customerId { get; set; }
        public int? role { get; set; }
        public string password { get; set; }
        public bool? active { get; set; }

    }
}
