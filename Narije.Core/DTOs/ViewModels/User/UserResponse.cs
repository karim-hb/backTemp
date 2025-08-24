using System;

namespace Narije.Core.DTOs.ViewModels.User
{
    public class UserResponse
    {
        public Int32 id { get; set; }
        public String fname { get; set; }
        public String lname { get; set; }
        public String description { get; set; }
        public String mobile { get; set; }
        public String customer { get; set; }
        public Int32? customerId { get; set; }
        public Int32? customerParentId { get; set; }
        public long? wallet { get; set; }
        public Int32 role { get; set; }
        public DateTime? lastLogin { get; set; }
        public Boolean? active { get; set; }
        public TimeSpan? cancelTime { get; set; }
        public TimeSpan? guestTime { get; set; }
        public TimeSpan? reserveTime { get; set; }
        public Boolean showPrice { get; set; }
        public Int32? accessId { get; set; }
        public String accessName { get; set; }
        public int payType { get; set; }
        public int foodType { get; set; }
        public String mealType { get; set; }
        public int? galleryId { get; set; }
        public Boolean gender { get; set; }
    
    }
}

