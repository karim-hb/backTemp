using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Controllers.Admin.Generic;
using Narije.Core.DTOs.Generic;
using Narije.Core.DTOs.ViewModels.Dish;
using Narije.Core.DTOs.ViewModels.Job;
using Narije.Core.Entities;
using Narije.Core.Interfaces.GenericRepository;

namespace Narije.Api.Controllers.Admin
{
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[Controller]")]
    public class DishController : BaseController<Dish, int, DishRequest, SameEntityResponse>
    {
        public DishController(IGenericRepository<Dish, int, DishRequest, SameEntityResponse> repository)
            : base(repository)
        {
        }
    }
}
    