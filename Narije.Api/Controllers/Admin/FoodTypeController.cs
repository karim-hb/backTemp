using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Narije.Api.Controllers.Admin.Generic;
using Narije.Core.DTOs.Generic;
using Narije.Core.DTOs.ViewModels.FoodType;
using Narije.Core.Entities;
using Narije.Core.Interfaces.GenericRepository;

namespace Narije.Api.Controllers.Admin
{
    [Authorize(Roles = "admin,supervisor")]
    [ApiController]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[Controller]")]
    public class FoodTypeController : BaseController<FoodType, int, FoodTypeRquest, SameEntityResponse>
    {
        public FoodTypeController(IGenericRepository<FoodType, int, FoodTypeRquest, SameEntityResponse> repository)
            : base(repository)
        {
        }
    }
}
