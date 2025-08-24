using System;
using System.Collections.Generic;
using System.Linq;


namespace Narije.Core.DTOs.Public
{
    /// <summary>
    /// ویو مدل OK Response
    /// </summary>
    public class ApiOkResponse : ApiResponse
    {
        /// <summary>
        /// ApiOkResponse
        /// </summary>
        public ApiOkResponse(string _Status = "OK", int _Code = 200, string _Message = null, object _Data = null, MetaResult _Meta = null, LinkResult _Links = null, List<FieldResponse> _Header = null, object _Setting = null, string _Sync = null, object _ExtraObject = null)
        //: base(_Status: _Status, _Code: _Code, _Message: _Message, _Data: _Data, _Meta: _Meta, _Links: _Links, _Header: _Header, _Setting: _Setting)
        {
            base.Data = _Data;
            base.Meta = _Meta;
            base.Links = _Links;
            base.Header = _Header;
            base.Status = "OK";
            base.Code = 200;
            base.Message = _Message;
            if (_Setting is null)
            {
                if (base.Header != null)
                {
                    var hasExtra = base.Header.Where(A => A.showInExtra).FirstOrDefault();
                    base.Setting = new
                    {
                        TableType = hasExtra == null ? "normal" : "accordian",
                        Sync = _Sync,
                        ExtraObject = _ExtraObject
                    };
                }
            }
            else
                base.Setting = _Setting;

        }
    }
}
