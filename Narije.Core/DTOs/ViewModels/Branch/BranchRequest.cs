using Microsoft.AspNetCore.Http;
using Narije.Core.DTOs.Generic;
using Narije.Core.Interfaces.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Branch
{
    public class BranchRequest : BaseRequest<int>, IFileRequest
    {
        public string title { get; set; }
        public string regNumber { get; set; }
        public string tel { get; set; }
        public string postalCode { get; set; }
        public string address { get; set; }
        public string nationalId { get; set; }
        public string description { get; set; }

        public string lat { get; set; }
        public string lng { get; set; }
        public int? galleryId { get; set; } = null;
        public List<IFormFile> files { get; set; }
    }
}
