using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Reserve
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public int Num { get; set; }
        public int FoodId { get; set; }
        public DateTime DateTime { get; set; }
        public int State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ReserveType { get; set; }
        public int Price { get; set; }
        public int FoodType { get; set; }
        public int PayType { get; set; }
        public int Subsidy { get; set; }
        public int MealType { get; set; }
        public int? MenuId { get; set; }
        public int? BranchId { get; set; }
        public int? MenuInfo { get; set; }
        public int? PriceType { get; set; }
        public string DeliverHour { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Food Food { get; set; }
        public virtual Menu Menu { get; set; }
       // public virtual MenuInfo MenuInfos { get; set; }
        public virtual User User { get; set; }
        public virtual Branch Branch { get; set; }
    }
}
