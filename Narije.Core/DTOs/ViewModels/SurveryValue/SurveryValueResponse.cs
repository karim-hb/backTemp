using System;

namespace Narije.Core.DTOs.ViewModels.SurveryValue
{
    public class SurveryValueResponse
    {
        public Int32 id { get; set; }
        public String title { get; set; }
        public Int32 value { get; set; }
        public Boolean active { get; set; }
        public Int32 itemId { get; set; }
   }
}

