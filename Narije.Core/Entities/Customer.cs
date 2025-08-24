using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Customer
    {
        public Customer()
        {
            FoodPrices = new HashSet<FoodPrice>();
            Invoices = new HashSet<Invoice>();
            Menus = new HashSet<Menu>();
            Reserves = new HashSet<Reserve>();
            Users = new HashSet<User>();
            Surveys = new HashSet<Survey>();
            Credits = new List<Credit>();
            CompanyMeals = new HashSet<CompanyMeal>();
            AccessoryCompanies = new HashSet<AccessoryCompany>();
        }

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
        public int? LastCreditId { get; set; }
        public string PostalCode { get; set; }
        public int? ParentId { get; set; }
        public int PayType { get; set; }
        public Int32 Subsidy { get; set; }
        public int? MaxMealCanReserve { get; set; }
        public string MealType { get; set; }
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

        public string DeliverFullName { get; set; }
        public string DeliverPhoneNumber { get; set; }
        public int? PriceType { get; set; }
        public int? AvragePrice { get; set; }
        public int? ShippingFee { get; set; }

        public virtual City City { get; set; }
        public virtual Province Province { get; set; }
        public virtual Job Job { get; set; } 
        public virtual Settlement Settlement { get; set; }
        public virtual Dish Dish { get; set; }

        public virtual ICollection<Menu> Menus { get; set; }
        public virtual ICollection<Reserve> Reserves { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<FoodPrice> FoodPrices { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<Survey> Surveys { get; set; }
        public virtual ICollection<Credit> Credits { get; set; }
        public virtual ICollection<CompanyMeal> CompanyMeals { get; set; } 
        public virtual ICollection<AccessoryCompany> AccessoryCompanies { get; set; } 
    }
}
