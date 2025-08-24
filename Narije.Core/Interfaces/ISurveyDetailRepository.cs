using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.SurveyDetail;

namespace Narije.Core.Interfaces
{
    public interface ISurveyDetailRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(SurveyDetailInsertRequest request);

            Task<ApiResponse> EditAsync(SurveyDetailEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

