using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class HrPersonFormField
    {
        public int Id { get; set; }
        public int FormFieldId { get; set; }
        public string PersonId { get; set; }
        public string Defaults { get; set; }
    }
}
