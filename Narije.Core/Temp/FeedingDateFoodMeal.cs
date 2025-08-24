using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class FeedingDateFoodMeal
    {
        public FeedingDateFoodMeal()
        {
            FeedingReserves = new HashSet<FeedingReserf>();
        }

        public int Id { get; set; }
        public int FoodId { get; set; }
        public int MealId { get; set; }
        public int SelfId { get; set; }
        public DateTime Date { get; set; }
        public bool? Active { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual FeedingFood Food { get; set; }
        public virtual FeedingMeal Meal { get; set; }
        public virtual FeedingSelf Self { get; set; }
        public virtual ICollection<FeedingReserf> FeedingReserves { get; set; }
    }
}
