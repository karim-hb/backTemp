using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.Menu
{
    public class MenuEditRequest
    {
        public Int32 id { get; set; }
        public Int32 customerId { get; set; }
        public DateTime dateTime { get; set; }
        public Int32 foodId { get; set; }
        public Int32 maxReserve { get; set; }
        public Int32 foodType { get; set; }
        public int? specialPrice { get; set; }
        public int? echoPrice { get; set; }
        public int? mealType { get; set; }

    }
}

