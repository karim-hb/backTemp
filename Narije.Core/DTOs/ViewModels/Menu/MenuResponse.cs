using System;

namespace Narije.Core.DTOs.ViewModels.Menu
{
    public class MenuResponse
    {
        public Int32 id { get; set; }
        public DateTime dateTime { get; set; }
        public Int32 foodId { get; set; }
        public Int32 maxReserve { get; set; }
        public Int32 foodType { get; set; }
        public String food { get; set; }
        public int? specialPrice { get; set; }
        public int? echoPrice { get; set; }
        public int? mealType { get; set; }

    }
}

