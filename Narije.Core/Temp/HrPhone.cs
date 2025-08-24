using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPhone
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Code { get; set; }
        public string NumberType { get; set; }
        public int Priority { get; set; }
        public int PhoneableId { get; set; }
        public string PhoneableType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
