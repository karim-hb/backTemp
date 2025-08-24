using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Controllers.Admin.Generic;
using Narije.Core.DTOs.Generic;
using Narije.Core.DTOs.ViewModels.Branch;
using Narije.Core.Entities;
using Narije.Core.Interfaces.GenericRepository;

namespace Narije.Api.Controllers.Admin
{
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[Controller]")]
    public class BranchController : BaseController<Branch, int, BranchRequest, SameEntityResponse>
    {
        public BranchController(IGenericRepository<Branch, int, BranchRequest, SameEntityResponse> repository)
            : base(repository)
        {
        }
    }
}
