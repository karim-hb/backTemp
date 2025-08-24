using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class FoodGroup
    {
        public FoodGroup()
        {
            Foods = new HashSet<Food>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int? GalleryId { get; set; }
        public Boolean InvoiceAddOn { get; set; }
        public string ArpaNumber { get; set; }
        public string Description { get; set; }
        public virtual ICollection<Food> Foods { get; set; }
        public virtual Gallery Gallery { get; set; }
    }
}
