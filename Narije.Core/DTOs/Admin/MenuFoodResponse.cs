using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class MenuFoodResponse
    {
        public int? id { get; set; }
        public int foodId { get; set; }
        public string food { get; set; }
        public string foodDescription { get; set; }
        public int foodGroupId { get; set; }
        public string foodGroup { get; set; }
        public int foodType { get; set; }
        public int? echoPrice { get; set; }
        public int? specialPrice { get; set; }
        public int maxReserve { get; set; }
        public string image { get; set; }
        public int? mealId { get; set; }
    }
}
