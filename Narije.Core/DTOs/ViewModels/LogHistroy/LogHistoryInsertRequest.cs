using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.LogHistroy
{
    public class LogHistoryInsertRequest
    {
        public string entityName { get; set; }
        public long entityId { get; set; }
        public DateTime dateTime { get; set; }
        public int userId { get; set; }
        public int source { get; set; }
        public int action { get; set; }
        public string changed { get; set; }
    }
}
