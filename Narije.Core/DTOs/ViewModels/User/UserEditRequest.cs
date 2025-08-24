using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.User
{
    public class UserEditRequest
    {
        public Int32 id { get; set; }
        public String fname { get; set; }
        public String lname { get; set; }
        public string description { get; set; }
        public String mobile { get; set; }
        public Int32? customerId { get; set; }
        public Int32? role { get; set; }
        public String password { get; set; }
        public Boolean? active { get; set; }
        public Int32? accessId { get; set; }
        public string fromGallery { get; set; }
        public List<IFormFile> files { get; set; }
        public Boolean gender { get; set; }
    }
}

