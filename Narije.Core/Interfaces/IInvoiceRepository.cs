using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Invoice;

namespace Narije.Core.Interfaces
{
    public interface IInvoiceRepository
    {
            Task<ApiResponse> GetAsync(int id);

            Task<ApiResponse> GetAllAsync(int? page, int? limit);

            Task<ApiResponse> DeleteAsync(int id);

            Task<ApiResponse> InsertAsync(InvoiceInsertRequest request);

            Task<ApiResponse> EditAsync(InvoiceEditRequest request);

            Task<ApiResponse> ExportAsync();
    }
}

