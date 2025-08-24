using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.MenuLog
{
    public class MenuLogResponse
    {
        public int id { get; set; }
        public int foodId { get; set; }
        public string foodName { get; set; }
        public int userId { get; set; }
        public string userName{ get; set; }
        public int? echoPriceBefore { get; set; }
        public int? echoPriceAfter { get; set; }
        public int? specialPriceBefore { get; set; }
        public int? specialPriceAfter { get; set; }
        public DateTime dateTime { get; set; }
        public int? menuInfoId { get; set; }
        public int? menuId { get; set; }
        public DateTime? MenuDateTime { get; set; }
    }
}
