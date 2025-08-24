using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Narije.Core.DTOs.Notification
{
    public class NotificationResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("notifiable_type")]
        public string NotifiableType { get; set; }

        [JsonProperty("notifiable_id")]
        public int? NotifiableId { get; set; }

        [JsonProperty("type_id")]
        public int TypeId { get; set; }

        [JsonProperty("data")]
        public NotificationDataResponse Data { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}
