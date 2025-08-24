using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Province;

namespace Narije.Core.Interfaces
{
    public interface IProvinceRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(ProvinceInsertRequest request);

            Task<ApiResponse> EditAsync(ProvinceEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

