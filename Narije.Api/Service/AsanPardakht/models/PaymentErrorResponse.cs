using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Narije.Infrastructure.Payment
{
    public class ErrorDetails
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("args")]
        public ErrorArguments Args { get; set; }
    }

    public class ErrorArguments
    {
        [JsonProperty("merchantConfigurationId")]
        public int MerchantConfigurationId { get; set; }
    }
}
