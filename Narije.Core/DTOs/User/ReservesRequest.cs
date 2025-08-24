using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.User
{
    public class ReservesRequest
    {
        public DateTime datetime { get; set; }
        public int mealId { get; set; }
        public int branchId { get; set; }
        public List<ReserveRequest> reserves { get; set; }
    }
}
