using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class ReportResponse
    {
        public int id { get; set; }
        public DateTime dateTime { get; set; }
        public int userId { get; set; }
        public int customerId { get; set; }
        public int state { get; set; }
        public string userName { get; set; }
        public string userDescription { get; set; }
        public string userMobile { get; set; }
        public string customer { get; set; }
        public int foodId { get; set; }
        public int foodGroupId { get; set; }
        public string food { get; set; }
        public string foodDescription { get; set; }
        public int mealType { get; set; }
        public string foodGroup { get; set; }
        public bool isFood { get; set; }
        public int qty { get; set; }
        public int? price { get; set; }
        public int foodType { get; set; }
    } 
}
