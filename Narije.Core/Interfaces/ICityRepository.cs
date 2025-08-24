using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.City;

namespace Narije.Core.Interfaces
{
    public interface ICityRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(CityInsertRequest request);

            Task<ApiResponse> EditAsync(CityEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

