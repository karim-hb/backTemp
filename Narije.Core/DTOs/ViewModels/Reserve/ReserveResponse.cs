using System;

namespace Narije.Core.DTOs.ViewModels.Reserve
{
    public class ReserveResponse
    {
        public Int32 id { get; set; }
        public Int32 userId { get; set; }
        public Int32 customerId { get; set; }
        public Int32 num { get; set; }
        public Int32 foodId { get; set; }
        public DateTime dateTime { get; set; }
        public Int32 state { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public Int32 reserveType { get; set; }
        public Int32 foodType { get; set; }
        public Int32 total { get; set; }
        public Int32 payType { get; set; }
        public Int32 price { get; set; }
        public int? customerParentId { get; set; }
        public String customer { get; set; }
        public String food { get; set; }
        public String user { get; set; }
        public Int32 score { get; set; }
        public Int32 mealType { get; set; }
        public int? branchId { get; set; }
        public int? menuId { get; set; }
        public int? menuInfo { get; set; }
        public bool hasSurvey { get; set; }
        public String foodTitle { get; set; }
        public Int32? foodGalleryId { get; set; }
        public String userName { get; set; }
        public String userDescription { get; set; }
        public String foodDescription { get; set; }
        public String category { get; set; }
        public String customerTitle { get; set; }
        public String menuInfoTitle { get; set; }
        public String mealTitle { get; set; }
        public String branchTitle { get; set; }
        public int? priceType { get; set; }
        public String deliverHour { get; set; }
        public String userMobile { get; set; }
        public String foodArpaNumber { get; set; }
        public int? productType { get; set; }

    }
}

