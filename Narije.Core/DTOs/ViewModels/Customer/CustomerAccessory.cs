using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Customer
{
    public class CustomerAccessory
    {
        public Int32 accessoryId { get; set; }
        public Int32 numbers { get; set; }
        public Int32? price { get; set; }
        public string title { get; set; }

    }
}
