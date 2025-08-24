using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.SurveyItem;

namespace Narije.Core.Interfaces
{
    public interface ISurveyItemRepository
    {
        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(SurveyItemInsertRequest request);

        Task<ApiResponse> EditAsync(SurveyItemEditRequest request);

        Task<ApiResponse> EditActiveAsync(int id);

        Task<ApiResponse> ExportAsync();
    }
}

