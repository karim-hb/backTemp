using System.Threading.Tasks;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Wallet;

namespace Narije.Core.Interfaces
{
    public interface IWalletRepository
    {
        Task<ApiResponse> ExportWalletAsync();

        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit);

        Task<ApiResponse> GetSumAsync();

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(WalletInsertRequest request);

        Task<ApiResponse> EditAsync(WalletEditRequest request);
        Task<ApiResponse> UpdateWalletsAsync(int customerId, int creditId);
    }
}

