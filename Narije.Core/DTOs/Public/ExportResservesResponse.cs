using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Public
{
    public class ExportResservesResponse
    {
        public int id { get; set; }
        public string dateTime { get; set; }
        public DateTime dt { get; set; }
        public int userId { get; set; }
        public string userName { get; set; }
        public string userMobile { get; set; }
        public int foodId { get; set; }
        public int customerId { get; set; }
        public string food { get; set; }
        public string foodGroup { get; set; }
        public int price { get; set; }
        public int qty { get; set; }
        public string foodType { get; set; }

    }
}
