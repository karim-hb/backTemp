using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Narije.Core.DTOs.Generic
{
    public class BaseRequest<T> : IBaseRequest<T>
    {
        [JsonProperty("id")]
        public T Id { get; set; } = default(T);
    }
    public interface IBaseRequest<T>
    {
        public T Id { get; set; }

    }

}
