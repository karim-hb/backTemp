using System;

namespace Narije.Core.DTOs.ViewModels.Recipts
{
    public class ReciptResponse
    {
        public int id { get; set; }
        public int userId { get; set; }
        public DateTime createdAt { get; set; }
        public int? customerId { get; set; }
        public string reserveIds { get; set; }
        public string fileName { get; set; }
        public int fileType { get; set; }
        public int? customerParentId { get; set; }
    }
}
