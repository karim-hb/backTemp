using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class MenuDayRequest
    {
        public DateTime datetime { get; set; }
        public List<MenuFoodRequest> foods { get; set; }
    }
}
