using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.FoodPrice;

namespace Narije.Core.Interfaces
{
    public interface IFoodPriceRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(FoodPriceInsertRequest request);

            Task<ApiResponse> EditAsync(FoodPriceEditRequest request);

            Task<ApiResponse> ProcessFoodPriceFileAsync(IFormFile file, int customerId);

            Task<ApiResponse> ExportAsync();
    }
}

