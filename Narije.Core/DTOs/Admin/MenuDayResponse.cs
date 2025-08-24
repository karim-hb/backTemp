using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class MenuDayResponse
    {
        public DateTime datetime { get; set; }
        public List<MenuFoodResponse> foods { get; set; }
    }
}
