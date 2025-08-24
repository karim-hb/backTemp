using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class vSurvey
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public int PositiveIndex { get; set; }
        public int NegativeIndex { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime ReserveTime { get; set; }
        public int Score { get; set; }
        public int? ReserveId { get; set; }
        public String Description { get; set; }
        public String user { get; set; }
        public String food { get; set; }
        public String Pos { get; set; }
        public String Neg { get; set; }
        public int? GalleryId { get; set; }

    }
}
