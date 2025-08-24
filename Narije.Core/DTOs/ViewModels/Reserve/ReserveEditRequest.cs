using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Reserve
{
    public class ReserveEditRequest
    {
        public Int32 id { get; set; }
        public Int32 userId { get; set; }
        public Int32 customerId { get; set; }
        public Int32 num { get; set; }
        public Int32 foodId { get; set; }
        public DateTime dateTime { get; set; }
        public Int32 state { get; set; }
        public Int32 reserveType { get; set; }
        public Int32 foodType { get; set; }
        public Int32 price { get; set; }
   }
}

