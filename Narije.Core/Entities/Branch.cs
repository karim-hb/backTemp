using Narije.Core.Seedwork;
using Newtonsoft.Json;


namespace Narije.Core.Entities
{
    public class Branch : BaseEntity<int>, IBaseGalaryEntity<int>
    {
        public Branch() { }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("regNumber")]
        public string RegNumber { get; set; }

        [JsonProperty("tel")]
        public string Tel { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }



        [JsonProperty("nationalId")]
        public string NationalId { get; set; }

        [JsonProperty("lat")]
        public string Lat { get; set; }

        [JsonProperty("lng")]
        public string Lng { get; set; }

        [JsonProperty("galleryId")]
        public int? GalleryId { get; set; }

    }
}
