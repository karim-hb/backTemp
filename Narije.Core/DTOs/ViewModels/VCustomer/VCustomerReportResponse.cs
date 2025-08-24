using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.VCustomer
{
    public class VCustomerReportResponse
    {
        public Int32 customerId { get; set; }
        public Int32 id { get; set; }
        public String title { get; set; }
        public string code { get; set; }
        public Int32? parentId { get; set; }
        public string parentCode { get; set; }
    }
}
