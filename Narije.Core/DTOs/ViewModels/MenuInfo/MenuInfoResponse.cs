using Narije.Core.DTOs.ViewModels.Customer;
using Narije.Core.DTOs.ViewModels.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.MenuInfo
{
    public class MenuInfoResponse
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public bool active { get; set; }
        public int? lastUpdaterUserId { get; set; }
        public string? lastUpdaterUser { get; set; }
        public string? customers { get; set; }
        public DateTime? updatedAt { get; set; }
    }
}
