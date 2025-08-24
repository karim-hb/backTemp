using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Narije.Core.DTOs.Extension
{
    public class ExtensionResponse
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("msg")]
        public object Msg { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("col")]
        public int Col { get; set; }

        [JsonProperty("route")]
        public string Route { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }
    }
}
