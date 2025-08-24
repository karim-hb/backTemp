using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Public
{
    public class NoticeResponse
    {
        [JsonProperty("params")]
        public object Params { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}