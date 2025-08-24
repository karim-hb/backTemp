using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Setting
    {
        public int Id { get; set; }
		public string CompanyName { get; set; }
		public string EconomicCode { get; set; }
		public string RegNumber { get; set; }
		public string Tel { get; set; }
		public string PostalCode { get; set; }
		public string Address { get; set; }
		public string NationalId { get; set; }
        public int? CompanyGalleryId { get; set; }
        public int? CompanyDarkGalleryId { get; set; }
        public string ContactMobile { get; set; }
        public int? CityId { get; set; }
        public int? ProvinceId { get; set; }
        public TimeSpan? SurveyTime { get; set; }
        public string PanelVersion { get; set; }
        public string FrontVersion { get; set; }
        public bool ForceSurvey { get; set; }
        public bool? PanelMaintenanceMode { get; set; }
        public bool? FrontMaintenanceMode { get; set; }
        public virtual City City { get; set; }
        public virtual Province Province { get; set; }

    }
}
