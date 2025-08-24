using System;
using System.Collections.Generic;
using System.Numerics;

namespace Narije.Core.Entities
{
    public partial class LogHistory
    {
        public LogHistory()
        {
           
        }

        public int Id { get; set; }
        public string EntityName { get; set; } 
        public long EntityId { get; set; } 
        public DateTime DateTime { get; set; }
        public int UserId { get; set; } 
        public int Source { get; set; } 
        public int Action { get; set; }
        public string Changed { get; set; } 

        public virtual User User { get; set; } 
    }
}