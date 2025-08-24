using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.AccessProfile;

namespace Narije.Core.Interfaces
{
    public interface IAccessProfileRepository
    {
        Task<ApiResponse> ExportAsync();

        Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(AccessProfileInsertRequest request);

            Task<ApiResponse> EditAsync(AccessProfileEditRequest request);
    }
}

