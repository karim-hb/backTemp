using Narije.Core.DTOs.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Tutorial
{
    public class TutorialRequest : BaseRequest<int>
    {
        public string title { get; set; }
        public string description { get; set; }
        public string href { get; set; }
        public string videoUrl { get; set; }
    }
}
