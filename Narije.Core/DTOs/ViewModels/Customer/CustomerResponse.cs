using System;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Customer
{
    public class CustomerResponse
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public TimeSpan? cancelTime { get; set; }
        public TimeSpan? guestTime { get; set; }
        public Int32 reserveAfter { get; set; }
        public Int32? reserveTo { get; set; }
        public Int32 cancelPercent { get; set; }
        public Int32 cancelPercentPeriod { get; set; }
        public DateTime contractStartDate { get; set; }
        public DateTime? contractEndDate { get; set; }
        public Boolean? addCreditToPrevCredit { get; set; }

        public Boolean active { get; set; }
        public String address { get; set; }
        public String tel { get; set; }
        public Int32 foodType { get; set; }
        public TimeSpan reserveTime { get; set; }
        public Boolean showPrice { get; set; }
        public Int32 minReserve { get; set; }
        public String economicCode { get; set; }
        public String nationalId { get; set; }
        public String regNumber { get; set; }
        public String mobile { get; set; }
        public Int32? cityId { get; set; }
        public Int32? provinceId { get; set; }
        public string mealType { get; set; }
        public String postalCode { get; set; }
        public Int32? parentId { get; set; }
        public String parent { get; set; }
        public String city { get; set; }
        public String province { get; set; }
        public int payType { get; set; }
        public Int32? maxMealCanReserve { get; set; }
        public Int32? jobId { get; set; }
        public Int32? settlementId { get; set; }
        public Int32? dishId { get; set; }
        public bool? isLegal { get; set; }
        public string code { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public string agentFullName { get; set; }
        public Int32? companyType { get; set; }
        public int? branchForSaturday { get; set; }
        public int? branchForSunday { get; set; }
        public int? branchForMonday { get; set; }
        public int? branchForTuesday { get; set; }
        public int? branchForWednesday { get; set; }
        public int? branchForThursday { get; set; }
        public int? branchForFriday { get; set; }

        public string deliverFullName { get; set; }
        public string deliverPhoneNumber { get; set; }
        public int? priceType { get; set; }
        public int? avragePrice { get; set; }
        public int? shippingFee { get; set; }


    }
}

