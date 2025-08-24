using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Narije.Core.DTOs.ViewModels.FoodPrice
{
    public class FoodPriceEditRequest
    {
        public int foodId { get; set; }
        public int customerId { get; set; }
        public int echoPrice { get; set; }
        public int specialPrice { get; set; }
    }
}

