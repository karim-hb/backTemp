using Narije.Core.Seedwork;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Entities
{

    public class Tutorial : BaseEntity<int>, IBaseEntity<int>
    {
        public Tutorial() { }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("videoUrl")]
        public string VideoUrl { get; set; }


    }
}
