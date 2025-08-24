using Narije.Core.Seedwork;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Entities
{
    public class Meal : BaseEntity<int>, IBaseGalaryEntity<int>
    {
        public Meal() { }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("galleryId")]
        public int? GalleryId { get; set; }
    }
}
