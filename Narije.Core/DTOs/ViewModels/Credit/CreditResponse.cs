using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Credit
{
    public class CreditResponse
    {
        public int id { get; set; }
        public int customerId { get; set; }
        public string customerTitle { get; set; }
        public DateTime dateTime { get; set; }
        public long value { get; set; }
        public bool riched { get; set; }
        public int year { get; set; }
        public int month { get; set; }
    }
}
