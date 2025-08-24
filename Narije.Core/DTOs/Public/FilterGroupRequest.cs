using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Narije.Core.DTOs.Public
{
    public class FilterRoot
    {
        public Filters filters { get; set; }
    }

    public class Filters
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("0")]
        public Filter _0 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("1")]
        public Filter _1 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("2")]
        public Filter _2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("3")]
        public Filter _3 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("4")]
        public Filter _4 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("5")]
        public Filter _5 { get; set; }
    }

    public class Filter
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("operator")]
        public string Operator { get; set; }

        [JsonPropertyName("value")]
        public object Value { get; set; }
    }
}
