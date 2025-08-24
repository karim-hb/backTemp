using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class FeedingMeal
    {
        public FeedingMeal()
        {
            FeedingDateFoodMeals = new HashSet<FeedingDateFoodMeal>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public bool? Active { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<FeedingDateFoodMeal> FeedingDateFoodMeals { get; set; }
    }
}
