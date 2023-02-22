using Application.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Shared.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize) where T : class
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageIndex, pageSize);
        }

    }
}
