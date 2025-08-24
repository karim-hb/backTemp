using Narije.Core.Seedwork;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Entities
{
    public class Settlement : BaseEntity<int>, IBaseEntity<int>
    {
        public Settlement() { }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    
    
    }
}
