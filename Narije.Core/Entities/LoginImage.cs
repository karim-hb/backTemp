using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Entities
{
    public class LoginImage
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? GalleryId { get; set; }
        public string Description { get; set; }
        public bool ForMobile { get; set; }
    }
}
