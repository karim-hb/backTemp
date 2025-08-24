using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrFormField
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public string Resource { get; set; }
        public string Validation { get; set; }
        public bool? Enabled { get; set; }
        public bool? Visible { get; set; }
        public bool Required { get; set; }
        public int Priority { get; set; }
        public string Props { get; set; }
        public string DefaultValue { get; set; }
        public string Values { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrForm Form { get; set; }
    }
}
