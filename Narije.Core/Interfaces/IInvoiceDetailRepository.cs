using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.InvoiceDetail;

namespace Narije.Core.Interfaces
{
    public interface IInvoiceDetailRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(InvoiceDetailInsertRequest request);

            Task<ApiResponse> EditAsync(InvoiceDetailEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

