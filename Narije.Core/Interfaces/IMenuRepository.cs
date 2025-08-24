using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Menu;

namespace Narije.Core.Interfaces
{
    public interface IMenuRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(MenuInsertRequest request);

            Task<ApiResponse> EditAsync(MenuEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

