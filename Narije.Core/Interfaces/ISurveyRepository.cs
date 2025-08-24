using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Survey;

namespace Narije.Core.Interfaces
{
    public interface ISurveyRepository
    {
        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit, bool byUser = false);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(SurveyInsertRequest request);

        Task<ApiResponse> EditAsync(SurveyEditRequest request);

        Task<ApiResponse> GetPositiveAsync(int? page, int? limit);

        Task<ApiResponse> GetNegativeAsync(int? page, int? limit);

        Task<ApiResponse> ParatelAsync();

        Task<ApiResponse> ExportNegativeAsync();

        Task<ApiResponse> ExportPositiveAsync();

        Task<ApiResponse> ExportParatelAsync();

        Task<ApiResponse> ExportAsync();
    }
}

