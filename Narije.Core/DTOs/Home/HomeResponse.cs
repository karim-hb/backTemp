using System;
using System.Text.Json.Serialization;

namespace Narije.Core.DTOs.Home
{
    public class HomeResponse
    {
        [JsonPropertyName("product_name")]
        public string ProductName { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("last_update")]
        public DateTime LastUpdate { get; set; }
    }
}