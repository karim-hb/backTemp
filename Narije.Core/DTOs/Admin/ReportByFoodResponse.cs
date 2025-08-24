using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class ReportByFoodResponse
    {
        public int foodId { get; set; }
        public string food { get; set; }
        public List<ReportResponse> items { get; set; }
    }
}
