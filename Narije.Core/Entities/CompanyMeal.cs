using System;

namespace Narije.Core.Entities
{
    public partial class CompanyMeal
    {
        public int Id { get; set; }
        public int MealId { get; set; }
        public int CustomerId { get; set; }
        public string MaxReserveTime { get; set; }
        public int MaxNumberCanReserve { get; set; }
        public string DeliverHour { get; set; }
        public bool? Active { get; set; }
        public int? FoodServerNumber { get; set; }
        public virtual Meal Meal { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
