using Narije.Core.DTOs.Generic;
using Narije.Core.Interfaces.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Job
{
    public class JobRequest : BaseRequest<int>
    {
        public string title { get; set; }
        public string description { get; set; }
    }
}
