using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrCountry
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
