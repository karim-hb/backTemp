using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Entities
{
    public partial class MenuLog
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public int UserId { get; set; }
        public int? EchoPriceBefore { get; set; }
        public int? EchoPriceAfter { get; set; }
        public int? SpecialPriceBefore { get; set; }
        public int? SpecialPriceAfter { get; set; }
        public DateTime DateTime { get; set; }
        public int MenuInfoId { get; set; }
        public int? MenuId { get; set; }
        public DateTime? MenuDateTime { get; set; }

        [ForeignKey("FoodId")]
        public virtual Food Food { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("MenuInfoId")]
        public virtual MenuInfo MenuInfo { get; set; }
    }
}
