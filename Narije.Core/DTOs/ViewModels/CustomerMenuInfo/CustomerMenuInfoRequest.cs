using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.CustomerMenuInfo
{
    public class CustomerMenuInfoRequest
    {
        public int id { get; set; }

        public int menuInfoId { get; set; }
        public int customerId { get; set; }
        public int month { get; set; }
        public int year { get; set; }
    }
}