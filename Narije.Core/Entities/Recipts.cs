using Narije.Core.Entities;
using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Recipt
    {
        public Recipt()
        {
         
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CustomerId { get; set; }
        public int? CustomerParentId { get; set; }
        public string ReserveIds { get; set; }
        public string FileName { get; set; }
        public int FileType { get; set; }

        public virtual User User { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Customer CustomerParent { get; set; }
    }
}
