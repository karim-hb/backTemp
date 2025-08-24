using System;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.FoodPrice
{
    public class FoodPriceInsertRequest
    {
        public Int32 foodId { get; set; }
        public Int32 customerId { get; set; }
        public Int32 echoPrice { get; set; }
        public Int32 specialPrice { get; set; }
   }
}

