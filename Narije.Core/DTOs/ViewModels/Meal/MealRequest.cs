using Microsoft.AspNetCore.Http;
using Narije.Core.DTOs.Generic;
using Narije.Core.Interfaces.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.DTOs.ViewModels.Meal
{
    public class MealRequest : BaseRequest<int>, IFileRequest
    {
        public string title { get; set; }
        public string description { get; set; }
        public int? galleryId { get; set; } = null;
        public List<IFormFile> files { get; set; }
    }

}
