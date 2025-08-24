using Narije.Core.DTOs.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Settlement
{
    public class SettlementRequest : BaseRequest<int>
    {
        public string title { get; set; }
        public string description { get; set; }
    }
}
