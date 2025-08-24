using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Search;

namespace Narije.Core.Interfaces
{
    public interface ISearchRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(SearchInsertRequest request);

            Task<ApiResponse> EditAsync(SearchEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

