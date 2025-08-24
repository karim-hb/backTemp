using Narije.Core.DTOs.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces
{
    public interface ICustomerWidget
    {
        Task<ApiResponse> GetSummary();
        Task<ApiResponse> GetReserves(DateTime fromDate, DateTime toDate);
    }
}
