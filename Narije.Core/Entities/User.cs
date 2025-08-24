using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class User
    {
        public User()
        {
            Reserves = new HashSet<Reserve>();
            Surveys = new HashSet<Survey>();
            WalletPayments = new HashSet<WalletPayment>();
            Wallets = new HashSet<Wallet>();
            MenuInfos = new HashSet<MenuInfo>();
            MenuLogs = new HashSet<MenuLog>();
        }

        public int Id { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Description { get; set; }
        public string Mobile { get; set; }
        public int? CustomerId { get; set; }
        public int Role { get; set; }
        public string Password { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool Active { get; set; }
        public int? AccessId { get; set; }
        public int? GalleryId { get; set; }
        public bool Gender { get; set; }
        public virtual AccessProfile Access { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<Reserve> Reserves { get; set; }
        public virtual ICollection<Survey> Surveys { get; set; }
        public virtual ICollection<WalletPayment> WalletPayments { get; set; }
        public virtual ICollection<Wallet> Wallets { get; set; }
        public virtual ICollection<LogHistory> LogHistory { get; set; }
        public virtual ICollection<MenuInfo> MenuInfos { get; set; }
        public virtual ICollection<MenuLog> MenuLogs { get; set; }

    }
}
