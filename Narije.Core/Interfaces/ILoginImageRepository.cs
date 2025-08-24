using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.LoginImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces
{
    public interface ILoginImageRepository
    {
        Task<ApiResponse> ExportAsync();

        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(LoginImageInsertRequest request);

        Task<ApiResponse> EditAsync(LoginImageEditRequest request);
    }
}
