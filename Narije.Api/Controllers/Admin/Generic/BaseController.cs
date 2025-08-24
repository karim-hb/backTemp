using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Narije.Api.Helpers;
using Narije.Core.Interfaces.GenericRepository;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Threading.Tasks;
using System;
using Narije.Core.DTOs.Public;

namespace Narije.Api.Controllers.Admin.Generic
{
    public abstract class BaseController<TEntity, TId, TRequest, TResponse> : ControllerBase where TEntity : class
    {
        protected readonly IGenericRepository<TEntity, TId, TRequest, TResponse> _repository;
        protected readonly string ParentIdName;
        protected readonly Expression<Func<TEntity, bool>>? _filter;
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? _includes;

        protected BaseController(IGenericRepository<TEntity, TId, TRequest, TResponse> repository,
            string? parentIdName = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includes = null,
            Expression<Func<TEntity, bool>>? filter = null)
        {
            _repository = repository;
            _includes = includes;
            ParentIdName = parentIdName;
            _filter = filter;
        }




        [HttpGet]
        [Route("Export")]
        public virtual async Task<IActionResult> Export()
        {
            try
            {
                return this.ServiceReturn(await _repository.ExportAsync(ParentIdName is not null ? ParentIdName : null));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace 
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }

        [HttpGet]
        [Route("Get")]
        public virtual async Task<IActionResult> Get(TId id)
        {
            try
            {
                return this.ServiceReturn(await _repository.GetAsync(id: id, _filter, _includes));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }

        [HttpGet]
        [Route("GetAll")]
        public virtual async Task<IActionResult> GetAll(int? page, int? limit, bool? active = null)
        {
            try
            {
                return this.ServiceReturn(await _repository.GetAllAsync(page: page, limit: limit, _filter, _includes,
                    active: active));
            }
            catch (Exception Ex)
            {
              

                return BadRequest(new ApiErrorResponse(_Code: StatusCodes.Status500InternalServerError, _Message: Ex.Message));
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public virtual async Task<IActionResult> Delete(TId id)
        {
            try
            {
                return this.ServiceReturn(await _repository.DeleteAsync(id: id));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }



        [HttpPost]
        [Route("DeleteAll")]
        public virtual async Task<IActionResult> DeleteAll([FromForm] List<TId> ids = null, [FromForm] int? parentId = null)
        {
            try
            {
                return this.ServiceReturn(await _repository.DeleteAllAsync(ids: ids, parentId: parentId));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }

        [HttpPost]
        [Route("EditActiveAll")]
        public virtual async Task<IActionResult> EditActiveAll([FromForm] bool activeValue, [FromForm] List<TId> ids = null, [FromForm] int? parentId = null)
        {
            try
            {
                return this.ServiceReturn(await _repository.UpdateAllActiveAsync(ids: ids, parentId: parentId, activeValue: activeValue));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public virtual async Task<IActionResult> Insert([FromForm] TRequest request)
        {
            try
            {
                return this.ServiceReturn(await _repository.InsertAsync(request: request));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }

        [HttpPut]
        [Route("Edit")]
        public virtual async Task<IActionResult> Edit([FromForm] TRequest request)
        {
            try
            {
                return this.ServiceReturn(await _repository.EditAsync(request: request));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }

        [HttpPut]
        [Route("EditActive")]
        public virtual async Task<IActionResult> EditActive([FromForm] TId id)
        {
            try
            {
                return this.ServiceReturn(await _repository.EditActiveAsync(id));
            }
            catch (Exception Ex)
            {
                var errorDetails = new
                {
                    ExceptionMessage = Ex.Message,
                    StackTrace = Ex.StackTrace
                };

                return StatusCode(StatusCodes.Status500InternalServerError, errorDetails);
            }
        }
    }
}
