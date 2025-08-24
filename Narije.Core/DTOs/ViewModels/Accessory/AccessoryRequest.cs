using Microsoft.AspNetCore.Http;
using Narije.Core.DTOs.Generic;
using Narije.Core.Interfaces.GenericRepository;
using Narije.Core.Seedwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Accessory
{
    public class AccessoryRequest : BaseRequest<int>, IFileRequest
    {
        public string title { get; set; }
        public string description { get; set; }
        public string arpaNumber { get; set; }
        public int? galleryId { get; set; } = null;
        public List<IFormFile> files { get; set; }
    }
    
}
