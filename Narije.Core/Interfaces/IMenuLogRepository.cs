using Narije.Core.DTOs.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces
{
    public interface IMenuLogRepository
    {
        Task<ApiResponse> GetAllAsync(int? page, int? limit , int menuInfo);
        Task<ApiResponse> ExportAsync();


    }
}
