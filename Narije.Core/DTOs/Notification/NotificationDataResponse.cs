using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Narije.Core.DTOs.Notification
{
    public class NotificationDataResponse
    {
        [JsonProperty("all")]
        public int All { get; set; }

        [JsonProperty("new")]
        public int New { get; set; }

        [JsonProperty("deviceName")]
        public string DeviceName { get; set; }
    }
}