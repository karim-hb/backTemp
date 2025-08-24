using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Narije.Core.DTOs.Public
{
    /// <summary>
    /// ویو مدل خروجی سرویس
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// Status
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        [JsonProperty("data")]
        public object Data { get; set; }

        /// <summary>
        /// Meta
        /// </summary>
        [JsonProperty("meta")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MetaResult Meta { get; set; }

        /// <summary>
        /// Meta
        /// </summary>
        [JsonProperty("extraObject")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object ExtraObject { get; set; }

        /// <summary>
        /// Links
        /// </summary>
        [JsonProperty("links")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public LinkResult Links { get; set; }

        /// <summary>
        /// Header
        /// </summary>
        [JsonProperty("header")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<FieldResponse> Header { get; set; }

        /// <summary>
        /// Setting
        /// </summary>
        [JsonProperty("setting")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Setting { get; set; }

        /// <summary>
        /// متد سازنده
        /// </summary>
        public ApiResponse() { }

        /// <summary>
        /// متد سازنده
        /// </summary>
        public ApiResponse(string _Status, int _Code, string _Message, object _Data, MetaResult _Meta, LinkResult _Links, object _ExtraObject)
        {
            Status = _Status;
            Code = _Code;
            Message = _Message;
            Data = _Data;
            Meta = _Meta;
            Links = _Links;
            ExtraObject = _ExtraObject;
        }
    }
}
