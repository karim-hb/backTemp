using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Reserve;

namespace Narije.Core.Interfaces
{
    public interface IReserveRepository
    {
        Task<ApiResponse> GetAsync(int id);

        Task<ApiResponse> GetAllAsync(int? page, int? limit, bool byUser = false, bool justPredict = false);

        Task<ApiResponse> GetAllByParamsAsync(int? page, int? limit, int paramsId, string paramsName , string headerName);
        
        Task<ApiResponse> DeleteAsync(int id);

        Task<ApiResponse> InsertAsync(ReserveInsertRequest request);

        Task<ApiResponse> EditAsync(ReserveEditRequest request);

        Task<FileContentResult> ExportBranchServicesAsync(DateTime fromData , DateTime toData , bool predict);

        Task<FileContentResult> ExportFoodBaseOnDayAsync(DateTime fromData, DateTime toData, bool isFood);

        Task<FileContentResult> ExportDailyBaseOnBranchesAndFoodAsync(DateTime fromData, DateTime toData, bool isFood);

        Task<FileContentResult> ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheFood(DateTime fromData, DateTime toData);
        Task<FileContentResult> ExportDifferenceBetweenPredictAndNormalReserveBaseOnTheBranches(DateTime fromData, DateTime toData);
        Task<FileContentResult> ExportDiffrenceBetweenPredictAndNormalReserveBaseOnTheCustomers(DateTime fromData, DateTime toData);


        Task<FileContentResult> ExportReserveBaseOnTheFood(DateTime fromData, DateTime toData, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false);
        Task<FileContentResult> ExportReserveBaseOnTheBranches(DateTime fromData, DateTime toData, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false);
        Task<FileContentResult> ExportReserveBaseOnTheCustomers(DateTime fromData, DateTime toData, string foodGroupIds = null, bool showAccessory = false, bool justPredict = false, bool isPdf = false);
        Task<ApiResponse> GetByFoodAsync();

        Task<ApiResponse> ExportAsync();
    }
}

