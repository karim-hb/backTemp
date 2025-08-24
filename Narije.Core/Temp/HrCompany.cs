using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrCompany
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CityId { get; set; }
        public int? CompanyId { get; set; }
        public int NatureOfBusinessId { get; set; }
        public string Code { get; set; }
        public string RegisterNumber { get; set; }
        public string NationalIdentificationCode { get; set; }
        public string EconomicCode { get; set; }
        public string Email { get; set; }
        public string EmailPassword { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Location { get; set; }
        public string Logo { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrCity City { get; set; }
    }
}
