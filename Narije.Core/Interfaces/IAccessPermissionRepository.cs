using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.AccessPermission;
using Narije.Core.DTOs.ViewModels.Export;

namespace Narije.Core.Interfaces
{
    public interface IAccessPermissionRepository
    {
        Task<ApiResponse> ExportAsync();

        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> GetAllByAccessIdAsync(int? page, int? limit, int accessId);

        Task<ApiResponse> DeleteAsync(int accessId, int permissionId);

        Task<ApiResponse> InsertAsync(AccessPermissionInsertRequest request);

        Task<ApiResponse> EditAsync(AccessPermissionEditRequest request);
    }
}

