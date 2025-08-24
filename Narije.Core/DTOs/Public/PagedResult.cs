using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Narije.Core.DTOs.Public
{
    /// <summary>
    /// پیج بندی
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T> where T : class
    {
        /// <summary>
        /// متد سازنده
        /// </summary>
        public PagedResult()
        {
            Data = new List<T>();
        }

        /// <summary>
        /// نتیجه
        /// </summary>
        [JsonProperty("data")]
        public IList<T> Data { get; set; }

        /// <summary>
        /// متا دیتا
        /// </summary>
        [JsonProperty("meta")]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MetaResult Meta { get; set; }
    }
}
