using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class vReserve
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public int Num { get; set; }
        public int FoodId { get; set; }
        public int Total { get; set; }

        public DateTime DateTime { get; set; }
        public int State { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int ReserveType { get; set; }
        public int Price { get; set; }
        public int FoodType { get; set; }
        public int PayType { get; set; }
        public int? Score { get; set; }
        public int MealType { get; set; }
        public bool HasSurvey { get; set; }
        public String FoodTitle { get; set; }
        public int? FoodGalleryId { get; set; }
        public int? BranchId { get; set; }
        public int? MenuId { get; set; }
        public int? MenuInfo { get; set; }
        public String UserName { get; set; }
        public String UserDescription { get; set; }
        public String FoodDescription { get; set; }
        public String Category { get; set; }
        public String CustomerTitle { get; set; }
        public String MenuInfoTitle { get; set; }
        public String MealTitle { get; set; }
        public String BranchTitle { get; set; }
        public String ProductTypeTitle { get; set; }
        public int? ProductType { get; set; }
        public bool Vip { get; set; }
        public bool IsFood { get; set; }
        public int? PriceType { get; set; }
        public int? MealImage { get; set; }
        public String FoodArpaNumber { get; set; }
        public String FoodGroupTitle { get; set; }
        public int? FoodVat { get; set; }
        public string DeliverHour { get; set; }
        public string UserMobile { get; set; }
        public int? CustomerParentId { get; set; }
        public int? FoodGroupId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerParentCode { get; set; }
        public string CustomerParentTitle { get; set; }

    }
}
