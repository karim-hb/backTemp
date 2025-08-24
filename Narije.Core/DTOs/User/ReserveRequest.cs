using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.User
{
    public class ReserveRequest
    {
        public int foodId { get; set; }
        public int foodType { get; set; }
        public int qty { get; set; }
        public int? price { get; set; }


    }
}
