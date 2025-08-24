using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.User
{
    public class ReserveResponse
    {
        public int id { get; set; }
        public int foodId { get; set; }
        public string food { get; set; }
        public string foodDescription { get; set; }
        public int foodGroupId { get; set; }
        public int foodType { get; set; }
        public string foodGroup { get; set; }
        public int maxReserve { get; set; }
        public int qty { get; set; }
        public int price { get; set; }
        public int echoPrice { get; set; }
        public int specialPrice { get; set; }
        public int? mealType { get; set; }
        public string state { get; set; }
        public string image { get; set; }
        public bool? fromMenu { get; set; }
        public bool? isFood { get; set; } 
        public bool? hasSurvey { get; set; }
        public int? reserveId { get; set; }
    }
}
