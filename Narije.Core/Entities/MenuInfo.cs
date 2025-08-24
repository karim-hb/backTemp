
using System;
using System.Collections.Generic;

namespace Narije.Core.Entities
{
    public partial class MenuInfo
    {
        public MenuInfo()
        {
            MenuLogs = new HashSet<MenuLog>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public bool Active { get; set; }
        public int? LastUpdaterUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual User LastUpdaterUser { get; set; }
        public virtual ICollection<MenuLog> MenuLogs { get; set; }
    }
}
