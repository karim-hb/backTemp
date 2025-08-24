using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class MenuRequest
    {
        public int? mealType;

        public int customerId { get; set; }
        public int mealId { get; set; }
        public List<MenuDayRequest> days { get; set; }
    }
}
