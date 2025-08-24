using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class MenuFoodRequest
    {
        public int foodId { get; set; }
        public int maxReserve { get; set; }
        public int foodType { get; set; }
        public int? mealId { get; set; }

    }
}
