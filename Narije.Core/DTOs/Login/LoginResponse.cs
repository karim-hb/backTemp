using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Login
{
    /// <summary>
    /// مدل پاسخ سرویس لاگین
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Type
        /// </summary>
        [JsonProperty("userId")]
        public int UserId { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Role
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; }

        /// <summary>
        /// AccessToken
        /// </summary>
        [JsonProperty("token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// ExpiresIn
        /// </summary>
        [JsonProperty("expire")]
        public int ExpiresIn { get; set; }

        [JsonProperty("secretKey")]
        public string SecretKey { get; set; }

        
    }
}
