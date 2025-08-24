using Microsoft.AspNetCore.Http;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Menu;
using Narije.Core.DTOs.ViewModels.MenuInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces
{
    public interface IMenuInfoRepository
    {
        Task<ApiResponse> GetAsync(int id); 

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(MenuInfoInserRequest request);

        Task<ApiResponse> EditAsync(MenuInfoEditRequest request);
        Task<ApiResponse> EditActiveAsync(int id);

        Task<ApiResponse> ImportFromExcelAsync(IFormFile file, MenuInfoEditRequest request, int MealType);

        Task<ApiResponse> ExportAsync();
    }
}
