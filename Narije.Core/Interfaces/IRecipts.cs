using Microsoft.AspNetCore.Mvc;
using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces
{
    public interface IRecipts
    {

        Task<ApiResponse> GetAllAsync(int? page, int? limit);
        Task<ApiResponse> ActiveReserve(string customerIds, DateTime date);
        Task<FileContentResult> ExportRecipt(string customerIds, DateTime date, bool all = false);
        Task<FileContentResult> ExportPdfRecipt(string customerIds, DateTime date, bool all = false);

        Task<ApiResponse> ExportAsync();
    }
}
