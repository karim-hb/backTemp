using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Client { get; set; }
        public string Route { get; set; }
        public string Parameters { get; set; }
        public string Method { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
