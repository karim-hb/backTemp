using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaDeviceSetting
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Props { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
