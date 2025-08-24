using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Wallet
    {
        public Wallet()
        {
            WalletPayments = new HashSet<WalletPayment>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
        public long PreValue { get; set; }
        public int Op { get; set; }
        public long Value { get; set; }
        public long RemValue { get; set; }
        public string Opkey { get; set; }
        public DateTime? LastCredit { get; set; }
        public int? LastCreditId { get; set; }

        public string Description { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<WalletPayment> WalletPayments { get; set; }
    }
}
