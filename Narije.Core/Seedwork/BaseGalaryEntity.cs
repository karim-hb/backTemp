using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Seedwork
{
    public class BaseGalaryEntity<T> : IBaseGalaryEntity<T>
    {
        [JsonProperty("id")]
        public T Id { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;

        [JsonProperty("creatorUserId")]
        public int CreatorUserId { get; set; }

        [JsonProperty("lastUpdaterUserId")]
        public int? LastUpdaterUserId { get; set; }
        public int? GalleryId { get; set; }
    }
}
