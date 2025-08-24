using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPerson
    {
        public HrPerson()
        {
            FeedingReserfPersonIdEatenNavigations = new HashSet<FeedingReserf>();
            FeedingReserfPersonIdOrderedNavigations = new HashSet<FeedingReserf>();
            HrPersonContracts = new HashSet<HrPersonContract>();
            HrPersonFinancialInfos = new HashSet<HrPersonFinancialInfo>();
            HrPersonInsurances = new HashSet<HrPersonInsurance>();
            HrPositions = new HashSet<HrPosition>();
            RqRequestRulePackUsers = new HashSet<RqRequestRulePackUser>();
            RqRequests = new HashSet<RqRequest>();
            RqWorkflowUsers = new HashSet<RqWorkflowUser>();
            TaBurntRepurchaseTransfers = new HashSet<TaBurntRepurchaseTransfer>();
            TaClockings = new HashSet<TaClocking>();
            TaMonthlyRemainingLeaves = new HashSet<TaMonthlyRemainingLeaf>();
            TaPolicyUsers = new HashSet<TaPolicyUser>();
            TaRemainingLeaveConfigs = new HashSet<TaRemainingLeaveConfig>();
            TaRemainingLeaves = new HashSet<TaRemainingLeave>();
            TaShiftMasks = new HashSet<TaShiftMask>();
            TaShiftPeople = new HashSet<TaShiftPerson>();
            TaWrits = new HashSet<TaWrit>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string PersonnelId { get; set; }
        public int CompanyId { get; set; }
        public int? IdentificationCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalCode { get; set; }
        public string FatherName { get; set; }
        public DateTime? Birthday { get; set; }
        public string BirthCertificateNumber { get; set; }
        public string Nationality { get; set; }
        public bool Married { get; set; }
        public bool Sex { get; set; }
        public string Address { get; set; }
        public int? ProvinceId { get; set; }
        public int? CityId { get; set; }
        public int? Education { get; set; }
        public int? Military { get; set; }
        public string Avatar { get; set; }
        public bool NightShift { get; set; }
        public string Props { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual HrCity City { get; set; }
        public virtual HrProvince Province { get; set; }
        public virtual HrUser User { get; set; }
        public virtual ICollection<FeedingReserf> FeedingReserfPersonIdEatenNavigations { get; set; }
        public virtual ICollection<FeedingReserf> FeedingReserfPersonIdOrderedNavigations { get; set; }
        public virtual ICollection<HrPersonContract> HrPersonContracts { get; set; }
        public virtual ICollection<HrPersonFinancialInfo> HrPersonFinancialInfos { get; set; }
        public virtual ICollection<HrPersonInsurance> HrPersonInsurances { get; set; }
        public virtual ICollection<HrPosition> HrPositions { get; set; }
        public virtual ICollection<RqRequestRulePackUser> RqRequestRulePackUsers { get; set; }
        public virtual ICollection<RqRequest> RqRequests { get; set; }
        public virtual ICollection<RqWorkflowUser> RqWorkflowUsers { get; set; }
        public virtual ICollection<TaBurntRepurchaseTransfer> TaBurntRepurchaseTransfers { get; set; }
        public virtual ICollection<TaClocking> TaClockings { get; set; }
        public virtual ICollection<TaMonthlyRemainingLeaf> TaMonthlyRemainingLeaves { get; set; }
        public virtual ICollection<TaPolicyUser> TaPolicyUsers { get; set; }
        public virtual ICollection<TaRemainingLeaveConfig> TaRemainingLeaveConfigs { get; set; }
        public virtual ICollection<TaRemainingLeave> TaRemainingLeaves { get; set; }
        public virtual ICollection<TaShiftMask> TaShiftMasks { get; set; }
        public virtual ICollection<TaShiftPerson> TaShiftPeople { get; set; }
        public virtual ICollection<TaWrit> TaWrits { get; set; }
    }
}
