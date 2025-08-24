using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Setting;

namespace Narije.Core.Interfaces
{
    public interface ISettingRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(SettingInsertRequest request);

            Task<ApiResponse> EditAsync(SettingEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

