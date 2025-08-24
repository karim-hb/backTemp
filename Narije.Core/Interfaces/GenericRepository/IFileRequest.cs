using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces.GenericRepository
{
    public interface IFileRequest
    {
        public int? galleryId { get; set; }
        public List<IFormFile> files { get; set; }
    }
}
