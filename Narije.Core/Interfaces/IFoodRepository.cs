using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Food;

namespace Narije.Core.Interfaces
{
    public interface IFoodRepository
    {
        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(FoodInsertRequest request);

        Task<ApiResponse> EditAsync(FoodEditRequest request);

        Task<ApiResponse> ProcessFoodFileAsync(IFormFile file);

        Task<ApiResponse> EditActiveAsync(int id);

        Task<ApiResponse> ExportAsync();
    }
}

