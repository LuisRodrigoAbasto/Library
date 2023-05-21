using Abasto.Dynamic.Interfaces;
using Abasto.Dynamic.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Abasto.Dynamic.DevExtreme
{
    public static class DevExtremeExtensions
    {
        public static async Task<IPaginateResult<T>> ToPaginateResultAsync<T>(this IQueryable<T> source, FilterDevExtreme filter, Action<QueryDevExtreme> options, CancellationToken cancellationToken = default) where T : class
        {
            return await source.PageResultAsync(filter: filter, options: options, cancellationToken: cancellationToken);
        }
        public static async Task<IPaginateResult<T>> ToPaginateResultAsync<T>(this IQueryable<T> source, string filter, Action<QueryDevExtreme> options, CancellationToken cancellationToken = default) where T : class
        {
            return await source.PageResultAsync(filter: filter, options: options, cancellationToken: cancellationToken);
        }
        public static async Task<IPaginateResult<T>> ToPaginateResultAsync<T>(this IQueryable<T> source, string filter) where T : class
        {
            return await source.PageResultAsync(filter: filter, options: null, cancellationToken: default);
        }
        public static IPaginateResult<T> ToPaginateResult<T>(this IQueryable<T> source, string filter, Action<QueryDevExtreme> options) where T : class
        {
            return source.PageResult(filter: filter, options: options);
        }
        public static async Task<IPaginateResult<T>> ToPaginateResultAsync<T>(this IQueryable<T> source, Action<QueryDevExtreme> options, CancellationToken cancellationToken = default) where T : class
        {
            return await source.PageResultAsync(filter: string.Empty, options: options, cancellationToken: cancellationToken);
        }
        public static IPaginateResult<T> ToPaginateResult<T>(this IQueryable<T> source, Action<QueryDevExtreme> options) where T : class
        {
            return source.PageResult(filter: string.Empty, options: options);
        }

        public static async Task<IPaginateResult<T>> ToPaginateResultAsync<T>(this IQueryable<T> source, FilterDevExtreme filter, CancellationToken cancellationToken = default) where T : class
        {
            return await source.PageResultAsync(filter: filter, options: null, cancellationToken: cancellationToken);
        }
        public static async Task<IPaginateResult<T>> ToPaginateResultAsync<T>(this IQueryable<T> source, string filter, CancellationToken cancellationToken = default) where T : class
        {
            return await source.PageResultAsync(filter: filter, options: null, cancellationToken: cancellationToken);
        }
        public static Task<IPaginateResult> ToDynamicPaginateResultAsync(this IEnumerable queryable, string filter, CancellationToken cancellationToken = default)
        {
            return PageResultAsync(queryable.AsQueryable(), filter: filter, options: null, cancellationToken: cancellationToken);
        }
        public static IPaginateResult ToDynamicPaginateResult(this IEnumerable source, string filter)
        {
            return PageResult(source.AsQueryable(), filter: filter, options: null);
        }
        public static IPaginateResult<T> ToPaginateResult<T>(this IQueryable<T> source, FilterDevExtreme filter) where T : class
        {
            return source.PageResult(filter: filter, options: null);
        }
        public static IPaginateResult<T> ToPaginateResult<T>(this IQueryable<T> source, string filter) where T : class
        {
            return source.PageResult(filter: filter, options: null);
        }
        public static IPaginateResult ToPaginateResult(this IQueryable source, FilterDevExtreme filter)
        {
            return source.PageResult(filter: filter, options: null);
        }

        private static Task<IPaginateResult<T>> PageResultAsync<T>(this IQueryable<T> source, FilterDevExtreme filter, Action<QueryDevExtreme> options, CancellationToken cancellationToken = default) where T : class
        {
            IPaginateExecute paginate = new PageResultProcess<T>(source, filter, options);
            IPaginateResultProcess<T> result = paginate as IPaginateResultProcess<T>;
            if (result != null) return result.PaginateResultAsync(cancellationToken);
            throw new Exception("<" + typeof(T)?.ToString() + ">");
        }
        private static Task<IPaginateResult<T>> PageResultAsync<T>(this IQueryable<T> source, string filter, Action<QueryDevExtreme> options, CancellationToken cancellationToken = default) where T : class
        {
            FilterDevExtreme filterClient = string.IsNullOrEmpty(filter) ? null : JsonConvert.DeserializeObject<FilterDevExtreme>(filter);
            IPaginateExecute paginate = new PageResultProcess<T>(source, filterClient, options);
            IPaginateResultProcess<T> result = paginate as IPaginateResultProcess<T>;
            if (result != null) return result.PaginateResultAsync(cancellationToken);
            throw new Exception("<" + typeof(T)?.ToString() + ">");
        }
        private static IPaginateResult<T> PageResult<T>(this IQueryable<T> source, FilterDevExtreme filter, Action<QueryDevExtreme> options) where T : class
        {
            IPaginateExecute paginate = new PageResultProcess<T>(source, filter, options);
            IPaginateResultProcess<T> result = paginate as IPaginateResultProcess<T>;
            if (result != null) return result.PaginateResult();
            throw new Exception("<" + typeof(T)?.ToString() + ">");
        }
        private static IPaginateResult<T> PageResult<T>(this IQueryable<T> source, string filter, Action<QueryDevExtreme> options) where T : class
        {
            FilterDevExtreme filterClient = string.IsNullOrEmpty(filter) ? null : JsonConvert.DeserializeObject<FilterDevExtreme>(filter);
            IPaginateExecute paginate = new PageResultProcess<T>(source, filterClient, options);
            IPaginateResultProcess<T> result = paginate as IPaginateResultProcess<T>;
            if (result != null) return result.PaginateResult();
            throw new Exception("<" + typeof(T)?.ToString() + ">");
        }
        private static Task<IPaginateResult> PageResultAsync(this IQueryable source, FilterDevExtreme filter, Action<QueryDevExtreme> options, CancellationToken cancellationToken = default)
        {
            IPaginateExecute paginate = new PageResultProcess(source, filter, options);
            IPaginateResultProcess result = paginate as IPaginateResultProcess;
            if (result != null) return result.PaginateResultAsync(cancellationToken);
            throw new Exception("<Error>");
        }
        private static Task<IPaginateResult> PageResultAsync(this IQueryable source, string filter, Action<QueryDevExtreme> options, CancellationToken cancellationToken = default)
        {
            FilterDevExtreme filterClient = string.IsNullOrEmpty(filter) ? null : JsonConvert.DeserializeObject<FilterDevExtreme>(filter);
            IPaginateExecute paginate = new PageResultProcess(source, filterClient, options);
            IPaginateResultProcess result = paginate as IPaginateResultProcess;
            if (result != null) return result.PaginateResultAsync(cancellationToken);
            throw new Exception("<Error>");
        }
        private static IPaginateResult PageResult(this IQueryable source, FilterDevExtreme filter, Action<QueryDevExtreme> options)
        {
            IPaginateExecute paginate = new PageResultProcess(source, filter, options);
            IPaginateResultProcess result = paginate as IPaginateResultProcess;
            if (result != null) return result.PaginateResult();
            throw new Exception("<Error>");
        }
        private static IPaginateResult PageResult(this IQueryable source, string filter, Action<QueryDevExtreme> options)
        {
            FilterDevExtreme filterClient = string.IsNullOrEmpty(filter) ? null : JsonConvert.DeserializeObject<FilterDevExtreme>(filter);
            IPaginateExecute paginate = new PageResultProcess(source, filterClient, options);
            IPaginateResultProcess result = paginate as IPaginateResultProcess;
            if (result != null) return result.PaginateResult();
            throw new Exception("<Error>");
        }
    }
}