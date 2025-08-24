using System;

namespace Narije.Core.Entities
{
    public partial class AccessoryCompany
    {
        public int Id { get; set; }
        public int AccessoryId { get; set; }
        public int CompanyId { get; set; }
        public int Numbers { get; set; }
        public int? Price { get; set; }

        public virtual Accessory Accessory { get; set; }
        public virtual Customer Company { get; set; }
    }
}
