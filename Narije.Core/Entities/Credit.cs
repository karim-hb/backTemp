using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Entities
{
    public partial class Credit
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime DateTime { get; set; }
        public long Value { get; set; }
        public bool Riched { get; set; }
        public virtual Customer Customer { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
