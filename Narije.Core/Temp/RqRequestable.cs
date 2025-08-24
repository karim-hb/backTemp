using System;
using System.Collections.Generic;

namespace Raden.Core.Temp
{
    public partial class RqRequestable
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public int RequestableId { get; set; }
        public string RequestableType { get; set; }

        public virtual RqRequest Request { get; set; }
    }
}
