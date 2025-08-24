using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrNotificationUser
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int NotificationId { get; set; }
        public byte Status { get; set; }
        public DateTime? ReadDatetime { get; set; }
        public int SenderModuleId { get; set; }
        public byte SenderStatus { get; set; }

        public virtual HrNotification Notification { get; set; }
        public virtual HrUser User { get; set; }
    }
}
