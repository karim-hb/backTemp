using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.FoodGroup;

namespace Narije.Core.Interfaces
{
    public interface IFoodGroupRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(FoodGroupInsertRequest request);

            Task<ApiResponse> EditAsync(FoodGroupEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

