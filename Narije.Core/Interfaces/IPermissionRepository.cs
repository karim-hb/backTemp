using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Permission;

namespace Narije.Core.Interfaces
{
    public interface IPermissionRepository
    {
        Task<ApiResponse> ExportAsync();

        Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

    }
}

