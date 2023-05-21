using Abasto.Dynamic.DevExtreme;
using Abasto.Dynamic.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Abasto.Dynamic.Interfaces
{
    public interface IPaginateExecute
    {
        Task<IPaginateResult<T>> PaginateExecuteResultAsync<T>(CancellationToken cancellationToken = default);
        IPaginateResult<T> PaginateExecuteResult<T>();
    }
    public interface IPaginateResultProcess<T>
    {
        Task<IPaginateResult<T>> PaginateResultAsync(CancellationToken cancellationToken = default);
        IPaginateResult<T> PaginateResult();
    }
    public interface IPaginateResultProcess
    {
        Task<IPaginateResult> PaginateResultAsync(CancellationToken cancellationToken = default);
        IPaginateResult PaginateResult();
    }
    public interface IPaginateQueryResult
    {
    }
    public class PageResultProcess<T> : PaginateResultProcess<T>, IPaginateExecute
    {
        public PageResultProcess(IQueryable<T> source, FilterDevExtreme filter, Action<QueryDevExtreme> options) : base(source, filter, options)
        {
        }
    }
    public class PageResultProcess : PaginateResultProcess, IPaginateExecute
    {
        public PageResultProcess(IQueryable source, FilterDevExtreme filter, Action<QueryDevExtreme> options) : base(source, filter, options)
        {
        }
    }
}
