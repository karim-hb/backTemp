using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class WalletPayment
    {
        public int Id { get; set; }
        public int Op { get; set; }
        public int UserId { get; set; }
        public long Value { get; set; }
        public int? WalletId { get; set; }
        public DateTime DateTime { get; set; }
        public int Status { get; set; }
        public string RefNumber { get; set; }
        public string Result { get; set; }
        public string Pan { get; set; }
        public string ConsumeCode { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? GalleryId { get; set; }
        public int? ApplicantId { get; set; }
        public int? ApplierId { get; set; }
        public string Description { get; set; }
        public string Bank { get; set; }
        public string AccountNumber { get; set; }
        public int Gateway { get; set; }
        public string Reason { get; set; }
        public virtual User User { get; set; }
        public virtual Wallet Wallet { get; set; }
    }
}
