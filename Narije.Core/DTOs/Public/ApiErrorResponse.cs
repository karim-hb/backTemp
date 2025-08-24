using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Public
{
    public class ApiErrorResponse : ApiResponse
    {
        /// <summary>
        /// ApiOkResponse
        /// </summary>
        public ApiErrorResponse(int _Code = -1, string _Message = "Error")
        : base(_Status: "ERROR", _Code: _Code, _Message: _Message, _Data: null, _Meta: null, _Links: null , _ExtraObject:null)
        {
        }
    }
}
