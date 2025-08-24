using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

using Narije.Core.DTOs.Public;
namespace Narije.Core.Interfaces.GenericRepository
{
    public interface IGenericRepository<T, TId, TRequest, TResponse> where T : class
    {
        Task<ApiResponse> GetAsync(TId id, Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

        public Task<ApiResponse> GetAllAsync(int? page, int? limit, Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
            Expression<Func<T, object>>? orderBy = null, bool? isDesc = null, bool? active = null);


        Task<ApiResponse> InsertAsync(TRequest request);

        Task<ApiResponse> EditAsync(TRequest request);

        Task<ApiResponse> EditActiveAsync(TId id);

        Task<ApiResponse> DeleteAsync(TId id);
        Task<ApiResponse> DeleteAllAsync(List<TId> ids = null, object parentId = null);

        Task<ApiResponse> ExportAsync(string ParentIdName = null, Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);
        Task<ApiResponse> ImportDataAsync(int companyId, List<IFormFile> files, List<string> fieldNames, string keyFieldName, bool isUpdate, Action<T, string, object> setField);

        Task<ApiResponse> UpdateAllActiveAsync(List<TId> ids = null, object parentId = null, bool activeValue = false);
        Task<IQueryable<TCollection>> GetCollectionAsync<TCollection>(
         Expression<Func<T, bool>> filterExpression,
         string collectionPropertyName) where TCollection : class;


    }
}
