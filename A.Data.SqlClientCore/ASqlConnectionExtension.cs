using A.Data.SqlClientCore.Extension;
using A.Data.SqlClientCore.Model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace A.Data.SqlClientCore
{
    public static class ASqlConnectionExtension
    {
        public static int SqlDataFill(this IASqlConnection context, DataSet dataSet, Action<ADbConfig<SqlParameter>> options)
        {
            IASqlExtension extension = Create(context);
            return extension.SqlDataFill(dataSet, options);
        }
        public static int SqlDataFill(this IASqlConnection context, DataTable dataTable, Action<ADbConfig<SqlParameter>> options)
        {
            IASqlExtension extension = Create(context);
            return extension.SqlDataFill(dataTable, options);
        }
        public static List<T> SqlData<T>(this IASqlConnection context, Action<ADbConfig<SqlParameter>> options)
        {
            IASqlExtension extension = Create(context);
            return extension.SqlData<T>(options);
        }
        public static Task<List<T>> SqlDataAsync<T>(this IASqlConnection context, Action<ADbConfig<SqlParameter>> options)
        {
            return Create(context).SqlDataAsync<T>(options);
        }
        public static DataTable SqlData(this IASqlConnection context, Action<ADbConfig<SqlParameter>> options)
        {
            return Create(context).SqlData(options);
        }
        public static Task<DataTable> SqlDataAsync(this IASqlConnection context, Action<ADbConfig<SqlParameter>> options)
        {
            return Create(context).SqlDataAsync(options);
        }
        public static Task<int> BulkInsertAsync(this IASqlConnection context, IEnumerable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkInsertAsync(data, bulk);
        }
        public static Task<int> BulkInsertAsync(this IASqlConnection context, DataTable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkInsertAsync(data, bulk);
        }
        public static int BulkInsert(this IASqlConnection context, IEnumerable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkInsert(data, bulk);
        }
        public static int BulkInsert(this IASqlConnection context, DataTable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkInsert(data, bulk);
        }
        public static Task<int> BulkUpdateAsync(this IASqlConnection context, DataTable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkUpdateAsync(data, bulk);
        }
        public static int BulkUpdate(this IASqlConnection context, IEnumerable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkUpdate(data, bulk);
        }
        public static int BulkUpdate(this IASqlConnection context, DataTable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkUpdate(data, bulk);
        }
        public static Task<int> BulkUpdateAsync(this IASqlConnection context, IEnumerable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkUpdateAsync(data, bulk);
        }
        public static Task<int> ExecuteNonQueryAsync(this IASqlConnection context, Action<ADbConfig<SqlParameter>> sql)
        {
            return Create(context).ExecuteNonQueryAsync(sql);
        }
        public static int ExecuteNonQuery(this IASqlConnection context, Action<ADbConfig<SqlParameter>> sql)
        {
            return Create(context).ExecuteNonQuery(sql);
        }
        public static Task<object> ExecuteScalarAsync(this IASqlConnection context, Action<ADbConfig<SqlParameter>> sql)
        {
            return Create(context).ExecuteScalarAsync(sql);
        }
        public static object ExecuteScalar(this IASqlConnection context, Action<ADbConfig<SqlParameter>> sql)
        {
            return Create(context).ExecuteScalar(sql);
        }
        public static Task<int> BulkDeleteAsync(this IASqlConnection context, DataTable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkDeleteAsync(data, bulk);
        }
        public static int BulkDelete(this IASqlConnection context, IEnumerable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkDelete(data, bulk);
        }
        public static int BulkDelete(this IASqlConnection context, DataTable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkDelete(data, bulk);
        }
        public static Task<int> BulkDeleteAsync(this IASqlConnection context, IEnumerable data, Action<ASqlConfig> bulk)
        {
            return Create(context).BulkDeleteAsync(data, bulk);
        }
        internal static IASqlExtension Create(IASqlConnection sqlConnection)
        {
            var extension = new ASqlExtension((ASqlConnection)sqlConnection) as IASqlExtension;
            return extension;
        }
    }
}
