using Narije.Core.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.User
{
    public class ReservesResponse
    {
        public DateTime datetime { get; set; }
        public List<ReserveResponse> reserves { get; set; }
    }
}
