using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Gallery
    {
        public Gallery()
        {
            Foods = new HashSet<Food>();
            FoodGroups = new HashSet<FoodGroup>();
        }

        public int Id { get; set; }
        public string OriginalFileName { get; set; }
        public string SystemFileName { get; set; }
        public string Source { get; set; }
        public string Alt { get; set; }
        public bool Hidden { get; set; }

        public virtual ICollection<Food> Foods { get; set; }
        public virtual ICollection<FoodGroup> FoodGroups { get; set; }
    }
}
