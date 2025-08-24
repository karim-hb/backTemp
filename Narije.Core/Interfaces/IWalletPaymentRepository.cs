using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.WalletPayment;

namespace Narije.Core.Interfaces
{
    public interface IWalletPaymentRepository
    {
        Task<ApiResponse> ExportAsync();

        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(WalletPaymentInsertRequest request);

        Task<ApiResponse> EditAsync(WalletPaymentEditRequest request);

        Task<ApiResponse> EditStateAsync(int id, int state);

        //Task<OrderBankInfoResponse> RecheckBankTransactionAsync(int wpId);

        Task<ApiResponse> EditStateCodeAsync(int id);

    }

}

