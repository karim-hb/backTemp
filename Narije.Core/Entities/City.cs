using Narije.Core.Entities;
using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class City
    {
        public City()
        {
            Customers = new HashSet<Customer>();
            Settings = new HashSet<Setting>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public int ProvinceId { get; set; }
        public int TransportFee { get; set; }

        public virtual Province Province { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<Setting> Settings { get; set; }
    }
}
