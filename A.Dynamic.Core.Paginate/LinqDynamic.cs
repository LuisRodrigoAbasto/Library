using A.Dynamic.Core.Paginate.Interfaces;
using A.Dynamic.Core.Paginate.Model;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace A.Dynamic.Core.Paginate
{
    public static class LinqDynamic
    {
        public static Task<IPaginateResult<T>> ToPaginateResultAsync<T>(this IQueryable<T> queryable, SearchFilter filter)
        {
            var result = PaginateAsync<T>(queryable, filter, true);
            return result;
        }
        public static Task<IPaginateResult> ToPaginateResultAsync(this IQueryable queryable, SearchFilter filter)
        {
            var result = PaginateAsync(queryable, filter, true);
            return result;
        }
        public static async Task<IPaginateResult> ToDynamicPaginateResultAsync(this IEnumerable queryable, SearchFilter filter)
        {
            var result = await PaginateAsync(queryable.AsQueryable(), filter, true);
            return result;
        }
        public static IPaginateResult<T> ToPaginateResult<T>(this IQueryable<T> queryable, SearchFilter filter)
        {
            var result = PaginateAsync<T>(queryable, filter, false).GetAwaiter().GetResult();
            return result;
        }
        public static async Task<IPaginateResult> ToDynamicPaginateResultAsync(this IEnumerable queryable, IDictionary<string, string> filter)
        {
            var filters = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchFilter>(JObject.FromObject(filter).ToString());
            var result = await PaginateAsync(queryable.AsQueryable(), filters, true);
            return result;
        }
        public static IPaginateResult<T> ToPaginateResult<T>(this IQueryable<T> queryable, IDictionary<string, string> filter)
        {
            var filters = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchFilter>(JObject.FromObject(filter).ToString());
            var result = PaginateAsync<T>(queryable, filters, false).GetAwaiter().GetResult();
            return result;
        }
        public static IPaginateResult ToPaginateResult(this IQueryable queryable, SearchFilter filter)
        {
            var result = PaginateAsync(queryable, filter, false).GetAwaiter().GetResult();
            return result;
        }
        public static async Task<IPaginateResult<T>> ToPaginateResultAsync<T>(this IQueryable<T> queryable)
        {
            var result = await PaginateAsync<T>(queryable, filter: new SearchFilter(), true);
            return result;
        }
        public static IPaginateResult ToPaginateResult(this IQueryable queryable)
        {
            var result = PaginateAsync<dynamic>(queryable, filter: new SearchFilter(), false).GetAwaiter().GetResult();
            return result;
        }
        private static async Task<IPaginateResult<T>> PaginateAsync<T>(IQueryable query, SearchFilter filter, bool async)
        {
            var result = new PaginateResult<T>();
            // if (!string.IsNullOrEmpty(filter.Where)) queryable = queryable.Where(filter.Where);

            IQueryable queryCount = query;

            if (!string.IsNullOrEmpty(filter.Sort)) query = query.OrderBy(filter.Sort);
            if (filter.IsLoadingAll != true)
            {
                if (filter.Skip != null) query = query.Skip(filter.Skip.Value);
                if (filter.Take != null) query = query.Take(filter.Take.Value);
            }
            result.Data = async ? await query.ToDynamicListAsync<T>() : query.ToDynamicList<T>();
            if (filter.IsLoadingAll != true && filter.RequireTotalCount == true)
            {
                if (filter.Take != null)
                {
                    result.TotalCount = result.Data.Count() < filter.Take ? result.Data.Count() : queryCount.Count();
                }
            }
            return result;
        }
        private static async Task<IPaginateResult> PaginateAsync(IQueryable queryable, SearchFilter filter, bool async)
        {
            var result = new PaginateResult();
            // if (!string.IsNullOrEmpty(filter.Where)) queryable = queryable.Where(filter.Where);
            IQueryable query;
            if (filter.Fields != null && filter.Fields.Any()) query = queryable.Select("new {" + string.Join(",", filter.Fields) + "}");
            else query = queryable;

            IQueryable queryCount = query;

            if (!string.IsNullOrEmpty(filter.Sort)) query = query.OrderBy(filter.Sort);
            if (filter.IsLoadingAll != true)
            {
                if (filter.Skip != null) query = query.Skip(filter.Skip.Value);
                if (filter.Take != null) query = query.Take(filter.Take.Value);
            }
            result.Data = async ? await query.ToDynamicListAsync() : query.ToDynamicList();
            if (filter.IsLoadingAll != true && filter.RequireTotalCount == true)
            {
                if (filter.Take != null)
                {
                    result.TotalCount = result.Data.AsQueryable().Count() < filter.Take ? result.Data.AsQueryable().Count() : queryCount.Count();
                }
            }
            return result;
        }
    }
}
