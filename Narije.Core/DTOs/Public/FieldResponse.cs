using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Public
{
    public class FieldResponse
    {
        public string name { get; set; }
        public string title { get; set; }
        public bool showInList { get; set; }
        public bool hasFilter { get; set; }
        public bool hasOrder { get; set; }
        public bool showInExtra { get; set; }
        public string type { get; set; }
        public string value { get; set; }
        public string style { get; set; }
        public string styleDark { get; set; }
        public string link { get; set; }
        [JsonIgnore]
        public int order { get; set; }
        public int filterOrder { get; set; }
        public int colSpan { get; set; }
        public string defaultFilter { get; set; }
        public List<EnumResponse> enums { get; set; }
    }
}
