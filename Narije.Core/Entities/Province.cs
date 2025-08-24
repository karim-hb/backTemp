using Narije.Core.Entities;
using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Province
    {
        public Province()
        {
            Customers = new HashSet<Customer>();
            Settings = new HashSet<Setting>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }

        public virtual ICollection<City> Cities { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
        public virtual ICollection<Setting> Settings { get; set; }
    }
}
