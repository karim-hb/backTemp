using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Credit
{
    public class CreditInsertArray
    {
        public int? Id { get; set; }
        public int? CustomerId { get; set; }
        public DateTime DateTime { get; set; }
        public long? Value { get; set; }
        public bool Riched { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
