using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.User;
using Narije.Core.DTOs.ViewModels.User;

namespace Narije.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(UserInsertRequest request);

        Task<ApiResponse> EditAsync(UserEditRequest request);

        Task<ApiResponse> ProcessUserFileAsync(IFormFile file);

        Task<ApiResponse> EditActiveAsync(int id);

        Task<ApiResponse> UserPermissionsAsync();

        Task<ApiResponse> ChangePasswordAsync(UserChangePasswordRequest request);

        Task<ApiResponse> ExportAsync();
    }
}

