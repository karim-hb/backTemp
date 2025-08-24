using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Customer;

namespace Narije.Core.Interfaces
{
    public interface ICustomerRepository
    {
        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit, bool? onlyBranch);

        Task<ApiResponse> GetAllCustomerReport(int? page, int? limit);

        Task<ApiResponse> GetAllCustomerMenuAsync(int customerId);

        Task<ApiResponse> GetAllBranchesAsync(int? page, int? limit , int companyId);

        Task<ApiResponse> GetLastCodeAsync( int? companyId);

        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(CustomerInsertRequest request);

        Task<ApiResponse> EditAsync(CustomerEditRequest request);

        Task<ApiResponse> EditActiveAsync(int id);

        Task<ApiResponse> ExportAsync(bool justBranches);

        Task<ApiResponse> ExportBranch(int companyId);

        Task UpdateCustomersAsync();

        Task<FileContentResult> CustomerAccessoryExport(int companyId);

        Task<FileContentResult> ExportBranchServicesAsync(DateTime fromData , DateTime toData , int customerId = 0, bool showProductId = false, bool showFoodType = false, bool showFoodGroup = false,
            bool showVat = false, bool showArpa = false, bool showQty = false, bool isFood = false);

    }
}

