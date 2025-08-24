using Narije.Core.DTOs.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces
{
    public interface IUserTurnoverRepository
    {
        Task<ApiResponse> ReportUserTurnoverAsync(int? page, int? limit, int userId, DateTime? fromDate, DateTime? toDate);
        Task<ApiResponse> ExportUserTurnoverAsync(int userId, DateTime? fromDate, DateTime? toDate);

    }
}
