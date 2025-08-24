using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.LogHistroy
{
    public class LogHistoryResponse
    {
        public int id { get; set; }
        public string entityName { get; set; }
        public long entityId { get; set; }
        public DateTime dateTime { get; set; }
        public string userName { get; set; }
        public int userId { get; set; }
        public int source { get; set; }
        public int action { get; set; }
        public string changed { get; set; }
    }
}
