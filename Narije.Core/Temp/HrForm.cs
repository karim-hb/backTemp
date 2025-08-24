using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrForm
    {
        public HrForm()
        {
            HrFormFields = new HashSet<HrFormField>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int FormableId { get; set; }
        public string FormableType { get; set; }
        public bool? Enabled { get; set; }
        public string Props { get; set; }
        public string Template { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual HrType Formable { get; set; }
        public virtual ICollection<HrFormField> HrFormFields { get; set; }
    }
}
