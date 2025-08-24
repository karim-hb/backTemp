using Narije.Core.DTOs.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Narije.Core.Entities;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Narije.Core.DTOs.Public
{
    /// <summary>
    /// ویو مدل OK Response
    /// </summary>
    public class Api300Response
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// ApiOkResponse
        /// </summary>
        public Api300Response(int _Code, string _Url)
        {
            this.Code = _Code;
            this.Url = _Url;
        }
    }
}
