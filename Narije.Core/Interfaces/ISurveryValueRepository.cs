using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.SurveryValue;

namespace Narije.Core.Interfaces
{
    public interface ISurveryValueRepository
    {
        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(SurveryValueInsertRequest request);

        Task<ApiResponse> EditAsync(SurveryValueEditRequest request);

        Task<ApiResponse> EditActiveAsync(int id);

        Task<ApiResponse> ExportAsync();
    }
}

