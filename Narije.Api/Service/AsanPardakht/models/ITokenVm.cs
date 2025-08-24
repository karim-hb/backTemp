using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Narije.Api.Payment.AsanPardakht.models
{


    public interface ITokenVm : IResponseVm
    {
         string RefId { get; set; }

    }

}