using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Enum
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public string Style { get; set; }
        public int ColumnOrder { get; set; }
        public string StyleDark { get; set; }
    }
}
