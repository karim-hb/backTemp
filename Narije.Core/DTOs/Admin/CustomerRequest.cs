using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class CustomerRequest
    {
        public int? id { get; set; }
        public string title { get; set; }
        //public string cancelTime { get; set; }
        public string guestTime { get; set; }
        public string reserveTime { get; set; }
        //public int reserveAfter { get; set; }
        public int? reserveTo { get; set; }
        //public int cancelPercent { get; set; }
        //public int cancelPercentPeriod { get; set; }
        public DateTime contractStartDate { get; set; }
        public bool active { get; set; }
        public string address { get; set; }
        public string tel { get; set; }
        public int foodType { get; set; }
        public bool showPrice { get; set; }
        public int minReserve { get; set; }
        public string economicCode { get; set; }
        public string nationalId { get; set; }
        public string regNumber { get; set; }
        public string mobile { get; set; }
        public string postalCode { get; set; }
        public int? cityId { get; set; }
        public int? provinceId { get; set; }
        public int? parentId { get; set; }
    }
}
