using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class ReportByDateResponse
    {
        public DateTime datetime { get; set; }
        public List<ReportResponse> items { get; set; }
    }
}
