using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Public
{
    public class LinkResult
    {
        [JsonProperty("GET")]
        public string Get { get; set; }

        [JsonProperty("DELETE")]
        public string Delete { get; set; }

        [JsonProperty("PUT")]
        public string Put { get; set; }

    }
}
