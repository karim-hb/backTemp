using Narije.Core.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class AdminReserveRequest
    {
        public int customerId { get; set; }
        public int? reserveType { get; set; }
        public int? mealId { get; set; }
        public DateTime datetime { get; set; }
        public List<ReserveRequest> reserves { get; set; }
    }
}
