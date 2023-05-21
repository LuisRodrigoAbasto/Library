using Abasto.Dynamic.Interfaces;
using Abasto.Dynamic.Model;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Abasto.Dynamic.DevExtreme
{
    public class PaginateResultProcess<T> : PaginateExecute, IPaginateResultProcess<T>
    {
        public PaginateResultProcess(IQueryable<T> source, FilterDevExtreme filter, Action<QueryDevExtreme> option) : base(source, filter, option)
        {
        }

        public async Task<IPaginateResult<T>> PaginateResultAsync(CancellationToken cancellationToken = default)
        {
            return await PaginateExecuteResultAsync<T>(cancellationToken);
        }
        public IPaginateResult<T> PaginateResult()
        {
            return PaginateExecuteResult<T>();
        }
    }
    public class PaginateResultProcess : PaginateExecute, IPaginateResultProcess
    {
        public PaginateResultProcess(IQueryable source, FilterDevExtreme filter, Action<QueryDevExtreme> option) : base(source, filter, option)
        {
        }

        public async Task<IPaginateResult> PaginateResultAsync(CancellationToken cancellationToken = default)
        {
            return await PaginateExecuteResultAsync<dynamic>(cancellationToken);
        }
        public IPaginateResult PaginateResult()
        {
            return PaginateExecuteResult<dynamic>();
        }

    }
}
