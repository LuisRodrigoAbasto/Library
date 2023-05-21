using A.Data.SqlClient.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace A.Data.SqlClient.Extension
{
    internal interface IASqlExtension : IDisposable
    {
        int BulkInsert(DataTable data, Action<ASqlConfig> bulk);
        int BulkInsert(IEnumerable data, Action<ASqlConfig> bulk);
        Task<int> BulkInsertAsync(DataTable data, Action<ASqlConfig> bulk);
        Task<int> BulkInsertAsync(IEnumerable data, Action<ASqlConfig> bulk);
        int SqlDataFill<T>(T data, Action<ADbConfig<SqlParameter>> options);
        DataTable SqlData(Action<ADbConfig<SqlParameter>> options);
        List<T> SqlData<T>(Action<ADbConfig<SqlParameter>> options);
        Task<DataTable> SqlDataAsync(Action<ADbConfig<SqlParameter>> options);
        Task<List<T>> SqlDataAsync<T>(Action<ADbConfig<SqlParameter>> options);
        Task<int> ExecuteNonQueryAsync(Action<ADbConfig<SqlParameter>> sql);
        int ExecuteNonQuery(Action<ADbConfig<SqlParameter>> sql);
        Task<object> ExecuteScalarAsync(Action<ADbConfig<SqlParameter>> sql);
        object ExecuteScalar(Action<ADbConfig<SqlParameter>> sql);
        int BulkUpdate(IEnumerable data, Action<ASqlConfig> bulk);
        Task<int> BulkUpdateAsync(IEnumerable data, Action<ASqlConfig> bulk);
        Task<int> BulkUpdateAsync(DataTable data, Action<ASqlConfig> bulk);
        int BulkUpdate(DataTable data, Action<ASqlConfig> bulk);
        int BulkDelete(IEnumerable data, Action<ASqlConfig> bulk);
        Task<int> BulkDeleteAsync(IEnumerable data, Action<ASqlConfig> bulk);
        Task<int> BulkDeleteAsync(DataTable data, Action<ASqlConfig> bulk);
        int BulkDelete(DataTable data, Action<ASqlConfig> bulk);
    }
}