using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class FoodPriceRequest
    {
        public int foodId { get; set; }
        public int customerId { get; set; }
        public int echoPrice { get; set; }
        public int specialPrice { get; set; }
    }
}
