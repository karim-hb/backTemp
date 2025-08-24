using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Entities
{
    public class CustomerMenuInfo
    {
        public int Id { get; set; }

        public int MenuInfoId { get; set; } 
        public int CustomerId { get; set; } 
        public int Month { get; set; } 
        public int Year { get; set; }

       
        public virtual MenuInfo MenuInfo { get; set; } 
        public virtual Customer Customer { get; set; }
    }
}
