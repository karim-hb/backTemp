using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class TaPolicyPosition
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public int PositionId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual TaPolicy Policy { get; set; }
        public virtual HrPosition Position { get; set; }
    }
}
