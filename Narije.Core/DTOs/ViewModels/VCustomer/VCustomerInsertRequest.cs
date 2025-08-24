using System;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.VCustomer
{
    public class VCustomerInsertRequest
    {
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
        public String postalCode { get; set; }
        public Int32? parentId { get; set; }
        public String city { get; set; }
        public String province { get; set; }
        public String parent { get; set; }
        public Int32? maxMealCanReserve { get; set; }
    }
}

