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
    public class MetaResult
    {
        /// <summary>
        /// تعداد کل رکورد ها
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        /// تعداد رکوردها در صفحه
        /// </summary>
        [JsonProperty("total_in_page")]
        public int TotalInPage { get; set; }

        /// <summary>
        /// تعداد صفحات
        /// </summary>
        [JsonProperty("total_pages")]
        public int TotalPage { get; set; }

        /// <summary>
        /// شماره صفحه جاری
        /// </summary>
        [JsonProperty("current_page")]
        public int CurrentPage { get; set; }

        /// <summary>
        /// تعداد رکورد ها در صفحات
        /// </summary>
        [JsonProperty("limit")]
        public int Limit { get; set; }

        /// <summary>
        /// تعداد رکورد ها در صفحات
        /// </summary>
        [JsonProperty("From")]
        public int? From
        {
            get { return (CurrentPage-1) * Limit + 1; }
        }

        /// <summary>
        /// تعداد رکورد ها در صفحات
        /// </summary>
        [JsonProperty("To")]
        public int? To
        {
            get { return Math.Min((CurrentPage + 1) * Limit, Total); }
        }

        /// <summary>
        /// تعداد رکورد ها در صفحات
        /// </summary>
        [JsonProperty("Next")]
        public int? Next { get; set; }

        /// <summary>
        /// تعداد رکورد ها در صفحات
        /// </summary>
        [JsonProperty("prev")]
        public int? Prev { get; set; }
    }
}
