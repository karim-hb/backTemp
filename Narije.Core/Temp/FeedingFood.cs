using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class FeedingFood
    {
        public FeedingFood()
        {
            FeedingDateFoodMeals = new HashSet<FeedingDateFoodMeal>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<FeedingDateFoodMeal> FeedingDateFoodMeals { get; set; }
    }
}
