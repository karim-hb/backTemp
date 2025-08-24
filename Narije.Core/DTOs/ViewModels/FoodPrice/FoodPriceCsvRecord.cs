using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.FoodPrice
{
    public class FoodPriceCsvRecord
    {
        public int? id { get; set; }
        public int foodId { get; set; }
        public Int32 echoPrice { get; set; }
        public Int32 specialPrice { get; set; }
    }
}
