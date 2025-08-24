using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Narije.Core.DTOs.Public;

namespace Narije.Infrastructure.Helpers
{
    public static class PagingHelper
    {

        /// <summary>
        /// تابع صفحه بندی
        /// </summary>
        public async static Task<PagedResult<T>> GetPaged<T>(this IQueryable<T> query, int Page, int Limit) where T : class
        {
            var Meta = new MetaResult()
            {
                CurrentPage = Page,
                Limit = Limit,
                Total = await query.CountAsync(),
                TotalInPage = Limit,
                Prev = Page - 1
            };

            if (Limit == 0)
                Limit = 100;
            var PageCount = (double)Meta.Total / Limit;
            Meta.TotalPage = (int)Math.Ceiling(PageCount);

            if (Meta.Prev < 0)
                Meta.Prev = null;

            var Result = new PagedResult<T>
            {
                Data = await query.Skip((Page - 1) * Limit).Take(Limit).ToListAsync(),
                Meta = Meta
            };

            return Result;
        }

        /// <summary>
        /// تابع صفحه بندی
        /// </summary>
        public static PagedResult<T> GetPagedSync<T>(this IQueryable<T> query, int Page, int Limit) where T : class
        {
            var Meta = new MetaResult()
            {
                CurrentPage = Page,
                Limit = Limit,
                Total = query.Count(),
                TotalInPage = Limit,
                Prev = Page - 1
            };

            if (Limit == 0)
                Limit = 100;
            var PageCount = (double)Meta.Total / Limit;
            Meta.TotalPage = (int)Math.Ceiling(PageCount);

            if (Meta.Prev < 0)
                Meta.Prev = null;

            var Result = new PagedResult<T>
            {
                Data = query.Skip((Page - 1) * Limit).Take(Limit).ToList(),
                Meta = Meta
            };

            return Result;
        }

    }
}