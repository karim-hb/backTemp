using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.Admin
{
    public class GalleryRequest
    {
        public int? Id { get; set; }
        public string OriginalFileName { get; set; }
        public string Source { get; set; }
        public string Alt { get; set; }
    }
}
