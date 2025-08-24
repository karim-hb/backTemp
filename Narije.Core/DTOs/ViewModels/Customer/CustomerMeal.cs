using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Customer
{
    public class CustomerMeal
    {
        public Int32 mealId { get; set; }
        public String maxReserveTime { get; set; }
        public Int32 maxNumberCanReserve { get; set; }
        public String deliverHour { get; set; }
        public Boolean? active { get; set; }
        public Int32? foodServerNumber { get; set; }
    }
}
