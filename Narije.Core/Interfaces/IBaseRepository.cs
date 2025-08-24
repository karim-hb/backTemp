using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Export;

namespace Narije.Core.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        Task<ApiResponse> ExportAsync();
    }
}
