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
        Task<FileContentResult> ExportRecipt(int? customerId, DateTime date);
        Task<FileContentResult> ExportPdfRecipt(int? customerId, DateTime date);

        Task<ApiResponse> ExportAsync();
    }
}
