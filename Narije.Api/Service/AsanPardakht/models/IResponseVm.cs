using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Api.Payment.AsanPardakht.models
{
    public interface IResponseVm
    {
        int ResCode { get; set; }
        string ResMessage { get; set; }
    }
}
