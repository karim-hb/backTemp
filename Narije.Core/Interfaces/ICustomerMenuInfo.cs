using Narije.Core.DTOs.Public;
using Narije.Core.DTOs.ViewModels.Credit;
using Narije.Core.DTOs.ViewModels.CustomerMenuInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narije.Core.Interfaces
{
     public interface ICustomerMenuInfo
    {
        Task<ApiResponse> GetAllAsync(int customerId);
        Task<ApiResponse> EditInsertAsync(CustomerMenuInfoRequest[] request);
    }
}
