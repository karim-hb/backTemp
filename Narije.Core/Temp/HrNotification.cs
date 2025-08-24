using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrNotification
    {
        public HrNotification()
        {
            HrNotificationUsers = new HashSet<HrNotificationUser>();
        }

        public int Id { get; set; }
        public string NotifiableType { get; set; }
        public int? NotifiableId { get; set; }
        public int TypeId { get; set; }
        public string Data { get; set; }
        public byte Status { get; set; }

        public virtual HrType Type { get; set; }
        public virtual ICollection<HrNotificationUser> HrNotificationUsers { get; set; }
    }
}
