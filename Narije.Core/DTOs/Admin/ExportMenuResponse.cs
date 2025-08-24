using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class ExportMenuResponse
    {
        public string datetime { get; set; }
        public DateTime dt { get; set; }
        public int foodId { get; set; }
        public string food { get; set; }
        public int foodGroupId { get; set; }
        public string foodGroup { get; set; }
        public string foodType { get; set; }
        public int maxReserve { get; set; }
        public string customer { get; set; }
    }
}
