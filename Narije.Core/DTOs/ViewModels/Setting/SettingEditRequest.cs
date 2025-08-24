using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Setting
{
    public class SettingEditRequest
    {
        public Int32 id { get; set; }
        public String companyName { get; set; }
        public String economicCode { get; set; }
        public String regNumber { get; set; }
        public String postalCode { get; set; }
        public String address { get; set; }
        public String tel { get; set; }
        public String nationalId { get; set; }
        public Int32? companyGalleryId { get; set; }
        public Int32? provinceId { get; set; }
        public Int32? cityId { get; set; }
        public String contactMobile { get; set; }
        public TimeSpan? surveyTime { get; set; }
        public List<IFormFile> files { get; set; }
        public Int32? companyDarkGalleryId { get; set; }
        public List<IFormFile> darkLogoFiles { get; set; }
        public bool? forceSurvey { get; set; }
        public bool? panelMaintenanceMode { get; set; }
        public bool? frontMaintenanceMode { get; set; }
    }
}

