using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class VCustomer
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public TimeSpan? CancelTime { get; set; }
        public TimeSpan? GuestTime { get; set; }
        public TimeSpan? ReserveTime { get; set; }
        public int ReserveAfter { get; set; }
        public int? ReserveTo { get; set; }
        public int CancelPercent { get; set; }
        public int CancelPercentPeriod { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public bool Active { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }
        public int FoodType { get; set; }
        public bool ShowPrice { get; set; }
        public bool? AddCreditToPrevCredit { get; set; }
        public int MinReserve { get; set; }
        public string EconomicCode { get; set; }
        public string NationalId { get; set; }
        public string RegNumber { get; set; }
        public string Mobile { get; set; }
        public int? CityId { get; set; }
        public int? ProvinceId { get; set; }
        public int? MealType { get; set; }
        public string PostalCode { get; set; }
        public int? ParentId { get; set; }
        public string Province { get; set; }
        public int? MaxMealCanReserve { get; set; }
        public string City { get; set; }
        public int PayType { get; set; }
        public Int32 Subsidy { get; set; }
        public int? JobId { get; set; }
        public int? SettlementId { get; set; }
        public int? DishId { get; set; }
        public bool? IsLegal { get; set; }
        public string Code { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string AgentFullName { get; set; }
        public int? CompanyType { get; set; }

        public int? BranchForSaturday { get; set; }
        public int? BranchForSunday { get; set; }
        public int? BranchForMonday { get; set; }
        public int? BranchForTuesday { get; set; }
        public int? BranchForWednesday { get; set; }
        public int? BranchForThursday { get; set; }
        public int? BranchForFriday { get; set; }
        public string ParentTitle { get; set; }
        public string DeliverFullName { get; set; }
        public string DeliverPhoneNumber { get; set; }
        public int? PriceType { get; set; }
        public int? AvragePrice { get; set; }
        public int? ShippingFee { get; set; }
        public string MealsTitle { get; set; }


    }
}
