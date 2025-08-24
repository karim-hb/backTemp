using System;
using System.Collections.Generic;

#nullable disable

namespace Narije.Core.Entities
{
    public partial class Header
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string FieldName { get; set; }
        public string Title { get; set; }
        public bool ShowInList { get; set; }
        public bool HasFilter { get; set; }
        public bool HasOrder { get; set; }
        public bool ShowInExtra { get; set; }
        public string ColumnType { get; set; }
        public string Style { get; set; }
        public int ColumnOrder { get; set; }
        public string Link { get; set; }
        public string DefaultFilter { get; set; }
        public int FilterOrder { get; set; }
        public int ColumnSpan { get; set; }
        public string StyleDark { get; set; }
        public bool Export { get; set; }
        public int ExportOrder { get; set; }
        public int AdminColumn { get; set; }
        public bool ImportUpdate { get; set; }
        public bool Import { get; set; }
        public string Icon { get; set; }
    }
}

