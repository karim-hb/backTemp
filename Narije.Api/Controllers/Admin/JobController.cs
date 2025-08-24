using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Controllers.Admin.Generic;
using Narije.Core.DTOs.Generic;
using Narije.Core.DTOs.ViewModels.Job;
using Narije.Core.Interfaces.GenericRepository;
using Narije.Core.Entities;

namespace Narije.Api.Controllers.Admin
{
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[Controller]")]
    public class JobController : BaseController<Job, int, JobRequest, SameEntityResponse>
    {
        public JobController(IGenericRepository<Job, int, JobRequest, SameEntityResponse> repository)
            : base(repository)
        {
        }
    }
}
