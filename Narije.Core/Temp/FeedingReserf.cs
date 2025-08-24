using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class FeedingReserf
    {
        public int Id { get; set; }
        public int DateFoodMealId { get; set; }
        public int PersonIdOrdered { get; set; }
        public int? PersonIdEaten { get; set; }
        public int? PositionIdOrdered { get; set; }
        public int? PositionIdEaten { get; set; }
        public int Count { get; set; }
        public byte Printed { get; set; }
        public string Description { get; set; }
        public int ClockingReasonId { get; set; }
        public string Status { get; set; }
        public string Props { get; set; }
        public byte ReserveType { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual FeedingDateFoodMeal DateFoodMeal { get; set; }
        public virtual HrPerson PersonIdEatenNavigation { get; set; }
        public virtual HrPerson PersonIdOrderedNavigation { get; set; }
        public virtual HrPosition PositionIdEatenNavigation { get; set; }
        public virtual HrPosition PositionIdOrderedNavigation { get; set; }
    }
}
