using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces
{
    public interface ICreditRepository
    {
        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(CreditInsertRequest request);

        Task<ApiResponse> EditAsync(CreditEditRequest request);

        Task<ApiResponse> ExportAsync();
    }
}
