using A.Data.SqlClient.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace A.Data.SqlClient.Extension
{
    internal class ASqlExtension : IASqlExtension, IDisposable
    {
        private ASqlConnection _dbConnection;
        public ASqlExtension(ASqlConnection sqlConnection)
        {
            _dbConnection = sqlConnection;
        }
        public int SqlDataFill<T>(T data, Action<ADbConfig<SqlParameter>> options)
        {
            return SqlFill(data, options);
        }
        public List<T> SqlData<T>(Action<ADbConfig<SqlParameter>> options)
        {
            var dataTable = SqlDataResultAsync<T>(options, false).GetAwaiter().GetResult();
            return (List<T>)dataTable;
        }
        public async Task<List<T>> SqlDataAsync<T>(Action<ADbConfig<SqlParameter>> options)
        {
            var dataTable = await SqlDataResultAsync<T>(options, true);
            return (List<T>)dataTable;
        }
        public DataTable SqlData(Action<ADbConfig<SqlParameter>> options)
        {
            DataTable data = new DataTable();
            SqlFill(data, options);
            return data;
        }
        public async Task<DataTable> SqlDataAsync(Action<ADbConfig<SqlParameter>> options)
        {
            var result = await SqlDataResultAsync<DataTable>(options, true);
            return (DataTable)result;
        }
        private Task<object> SqlDataResultAsync<T>(Action<ADbConfig<SqlParameter>> options, bool isAsync)
        {
            return SqlDataObjectAsync<T>(options, isAsync);
        }
        private Task<object> SqlDataObjectAsync<T>(Action<ADbConfig<SqlParameter>> options, bool isAsync)
        {
            var sql = new ADbConfig<SqlParameter>();
            options?.Invoke(sql);
            return SqlDataResultAsync<T>(sql, isAsync);
        }
        private async Task<object> SqlDataResultAsync<T>(ADbConfig<SqlParameter> sql, bool isAsync)
        {
            object data = null;
            if (isAsync)
            {
                using (SqlCommand command = GetSqlCommand(sql))
                {
                    try
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            try
                            {
                                List<DataColumn> columns = new List<DataColumn>();
                                for (int index = 0; index < reader.VisibleFieldCount; index++)
                                {
                                    var type = reader.GetFieldType(index);
                                    var columnName = reader.GetName(index);
                                    columns.Add(new DataColumn(columnName, type));
                                }
                                if (typeof(T) == typeof(DataTable))
                                {
                                    var dataTable = new DataTable();
                                    dataTable.Columns.AddRange(columns.ToArray());
                                    while (await reader.ReadAsync())
                                    {
                                        DataRow row = dataTable.NewRow();
                                        foreach (DataColumn prop in columns)
                                        {
                                            if (reader[prop.ColumnName] != DBNull.Value)
                                            {
                                                row[prop.ColumnName] = reader[prop.ColumnName];
                                            }
                                        }
                                        dataTable.Rows.Add(row);
                                    }
                                    data = dataTable;
                                }
                                else
                                {
                                    List<T> list = new List<T>();
                                    T obj = default;
                                    while (await reader.ReadAsync())
                                    {
                                        obj = Activator.CreateInstance<T>();
                                        foreach (PropertyInfo prop in (from x in obj.GetType().GetProperties()
                                                                       join col in columns on x.Name equals col.ColumnName
                                                                       select x))
                                        {
                                            if (!Equals(reader[prop.Name], DBNull.Value))
                                            {
                                                prop.SetValue(obj, reader[prop.Name], null);
                                            }
                                        }
                                        list.Add(obj);
                                    }
                                    data = list;
                                }
                            }
                            finally
                            {
                                reader.Close();
                            }

                        }
                    }
                    finally
                    {
                        command.Dispose();
                    }
                }
            }
            else
            {
                var datatable = new DataTable();
                SqlFillData(datatable, sql);
                if (typeof(T) != typeof(DataTable))
                {
                    List<T> list = new List<T>();
                    var columnas = new List<string>();
                    foreach (DataColumn col in ((DataTable)data).Columns) columnas.Add(col.ColumnName);

                    foreach (DataRow row in ((DataTable)data).Rows)
                    {
                        T obj = Activator.CreateInstance<T>();

                        foreach (PropertyInfo prop in (from x in obj.GetType().GetProperties()
                                                       join col in columnas on x.Name equals col
                                                       select x))
                        {
                            if (!Equals(row[prop.Name], DBNull.Value))
                            {
                                prop.SetValue(obj, row[prop.Name], null);
                            }
                        }
                        list.Add(obj);
                    }
                    data = list;
                }
                else
                {
                    data = datatable;
                }
            }

            return data;
        }
        private SqlCommand GetSqlCommand(ADbConfig<SqlParameter> sql)
        {
            var command = _dbConnection.DbTransaction == null ? new SqlCommand(sql.SqlQuery, _dbConnection.DbConnection)
            : new SqlCommand(sql.SqlQuery, _dbConnection.DbConnection, _dbConnection.DbTransaction);
            command.CommandTimeout = sql.Timeout;
            command.CommandType = sql.CommandType;
            if (sql.Parameters != null) command.Parameters.AddRange(sql.Parameters.ToArray());
            return command;
        }
        private SqlDataAdapter GetSqlDataAdapter(ADbConfig<SqlParameter> sql, SqlCommand sqlCommand)
        {
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            sqlDataAdapter.SelectCommand.CommandTimeout = sql.Timeout;
            sqlDataAdapter.SelectCommand.CommandType = sql.CommandType;
            return sqlDataAdapter;
        }
        private int SqlFill<T>(T data, Action<ADbConfig<SqlParameter>> options)
        {
            ADbConfig<SqlParameter> sql = new ADbConfig<SqlParameter>();
            options?.Invoke(sql);
            return SqlFillData(data, sql);
        }
        private int SqlFillData(object data, ADbConfig<SqlParameter> sql)
        {
            int result = 0;
            using (SqlCommand command = GetSqlCommand(sql))
            {
                try
                {
                    using (SqlDataAdapter sqlDataAdapter = GetSqlDataAdapter(sql, command))
                    {
                        try
                        {
                            if (data.GetType() == typeof(DataTable))
                            {
                                DataTable dataTable = (DataTable)data;
                                result = sqlDataAdapter.Fill(dataTable);
                                data = dataTable;
                            }
                            else
                            {
                                DataSet DataSet = (DataSet)data;
                                result = sqlDataAdapter.Fill(DataSet);
                            }
                        }
                        finally
                        {
                            sqlDataAdapter.Dispose();
                        }
                    }
                }
                finally
                {
                    command.Dispose();
                }
            }
            return result;
        }
        public int ExecuteNonQuery(Action<ADbConfig<SqlParameter>> sql)
        {
            return ExecuteNonQueryAsync(sql, false).GetAwaiter().GetResult();
        }
        public Task<int> ExecuteNonQueryAsync(Action<ADbConfig<SqlParameter>> sql)
        {
            return ExecuteNonQueryAsync(sql, true);
        }
        private Task<int> ExecuteNonQueryAsync(Action<ADbConfig<SqlParameter>> sql, bool isAsync)
        {
            var config = new ADbConfig<SqlParameter>();
            sql?.Invoke(config);
            return SqlExecuteNonQueryAsync(config, isAsync);
        }
        private async Task<int> SqlExecuteNonQueryAsync(ADbConfig<SqlParameter> sql, bool isAsync)
        {
            var task = SqlExecuteAsync(sql, ExecuteType.ExecuteNonQuery, isAsync);
            var result = isAsync ? await task : task.GetAwaiter().GetResult();
            return (int)result;
        }
        public object ExecuteScalar(Action<ADbConfig<SqlParameter>> sql)
        {
            return ExecuteScalarAsync(sql, false).GetAwaiter().GetResult();
        }
        public Task<object> ExecuteScalarAsync(Action<ADbConfig<SqlParameter>> sql)
        {
            return ExecuteScalarAsync(sql, true);
        }
        private Task<object> ExecuteScalarAsync(Action<ADbConfig<SqlParameter>> sql, bool isAsync)
        {
            var sqlConfig = new ADbConfig<SqlParameter>();
            sql?.Invoke(sqlConfig);
            return SqlExecuteAsync(sqlConfig, ExecuteType.ExecuteScalar, isAsync);
        }
        private async Task<object> SqlExecuteAsync(ADbConfig<SqlParameter> sql, ExecuteType type, bool isAsync)
        {
            object result = null;
            using (SqlCommand command = GetSqlCommand(sql))
            {
                try
                {
                    switch (type)
                    {
                        case ExecuteType.ExecuteScalar:
                            {
                                result = isAsync ? await command.ExecuteScalarAsync() : command.ExecuteScalar();
                            }
                            break;
                        case ExecuteType.ExecuteNonQuery:
                            {
                                result = isAsync ? await command.ExecuteNonQueryAsync() : command.ExecuteNonQuery();
                            }
                            break;
                    }
                }
                finally
                {
                    command.Dispose();
                }
            }
            return result;
        }
        ///column Imput es true=a las columnas de entrada, false=ignorar columnas de entradas
        public Task<int> BulkInsertAsync(IEnumerable data, Action<ASqlConfig> bulk)
        {
            return BulkInsertAsync(data, bulk, true);
        }
        public Task<int> BulkInsertAsync(DataTable data, Action<ASqlConfig> bulk)
        {
            return BulkInsertAsync(data, bulk, true);
        }
        public int BulkInsert(IEnumerable data, Action<ASqlConfig> bulk)
        {
            return BulkInsertAsync(data, bulk, false).GetAwaiter().GetResult();
        }
        public int BulkInsert(DataTable data, Action<ASqlConfig> bulk)
        {
            return BulkInsertAsync(data, bulk, false).GetAwaiter().GetResult();
        }
        private Task<int> BulkInsertAsync(IEnumerable data, Action<ASqlConfig> bulk, bool isAsync)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            var datatable = ToDataTable(data, bulkConfig);
            return BulkInsertAsync(datatable, bulkConfig, isAsync);
        }
        private Task<int> BulkInsertAsync(DataTable data, Action<ASqlConfig> bulk, bool isAsync)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            bulkConfig = ConvertExpression(bulkConfig, data);
            return BulkInsertAsync(data, bulkConfig, isAsync);
        }
        private async Task<int> BulkInsertAsync(DataTable data, ASqlConfig bulkConfig, bool isAsync)
        {
            if (data.Rows.Count == 0) return 0;
            using (SqlBulkCopy bulkCopy = GetSqlBulkCopy(bulkConfig, bulkConfig.DestinationTableName))
            {
                try
                {
                    if (isAsync)
                        await BulkCopyAsync(data, bulkCopy, isAsync);
                    else
                        BulkCopyAsync(data, bulkCopy, isAsync).GetAwaiter().GetResult();
                }
                finally
                {
                    bulkCopy.Close();
                }
            }
            return data.Rows.Count;
        }
        public int BulkUpdate(IEnumerable data, Action<ASqlConfig> bulk)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            var datatable = ToDataTable(data, bulkConfig);
            return BulkUpdateAsync(datatable, bulkConfig, false).GetAwaiter().GetResult();
        }

        public Task<int> BulkUpdateAsync(IEnumerable data, Action<ASqlConfig> bulk)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            var datatable = ToDataTable(data, bulkConfig);
            return BulkUpdateAsync(datatable, bulkConfig, true);
        }
        public Task<int> BulkUpdateAsync(DataTable data, Action<ASqlConfig> bulk)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            bulkConfig = ConvertExpression(bulkConfig, data);
            return BulkUpdateAsync(data, bulkConfig, true);
        }
        public int BulkUpdate(DataTable data, Action<ASqlConfig> bulk)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            bulkConfig = ConvertExpression(bulkConfig, data);
            return BulkUpdateAsync(data, bulkConfig, false).GetAwaiter().GetResult();
        }
        private async Task<int> BulkUpdateAsync(DataTable data, ASqlConfig bulkConfig, bool isAsync)
        {
            if (data.Rows.Count == 0) return 0;
            int result = 0;
            var ConfigTableTemp = isAsync ? await CreateQueryUpdateTemp(bulkConfig, isAsync) : CreateQueryUpdateTemp(bulkConfig, isAsync).GetAwaiter().GetResult();
            try
            {
                var taskQuery = ExecuteScalarAsync(x =>
                                    {
                                        x.SqlQuery = ConfigTableTemp.CreateTableQuery;
                                        x.CommandType = CommandType.Text;
                                    }, isAsync);
                if (isAsync) await taskQuery;
                else taskQuery.GetAwaiter().GetResult();

                using (SqlBulkCopy bulkCopy = GetSqlBulkCopy(bulkConfig, ConfigTableTemp.TableName))
                {
                    try
                    {
                        if (isAsync) await BulkCopyAsync(data, bulkCopy, isAsync);
                        else BulkCopyAsync(data, bulkCopy, isAsync).GetAwaiter().GetResult();
                    }
                    finally
                    {
                        bulkCopy.Close();
                    }
                }
                var taskResult = ExecuteNonQueryAsync(x =>
                                                              {
                                                                  x.SqlQuery = ConfigTableTemp.ExecuteQuery;
                                                                  x.CommandType = CommandType.Text;
                                                              }, isAsync);

                result = isAsync ? await taskResult : taskResult.GetAwaiter().GetResult();
            }
            finally
            {
                var taskDrop = ExecuteScalarAsync(x =>
                                   {
                                       x.SqlQuery = $"drop table {ConfigTableTemp.TableName}";
                                       x.CommandType = CommandType.Text;
                                   }, isAsync);
                if (isAsync) await taskDrop;
                else taskDrop.GetAwaiter().GetResult();
            }
            return result;
        }
        public int BulkDelete(IEnumerable data, Action<ASqlConfig> bulk)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            var datatable = ToDataTable(data, bulkConfig);
            return BulkUpdateAsync(datatable, bulkConfig, false).GetAwaiter().GetResult();
        }

        public Task<int> BulkDeleteAsync(IEnumerable data, Action<ASqlConfig> bulk)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            var datatable = ToDataTable(data, bulkConfig);
            return BulkDeleteAsync(datatable, bulkConfig, true);
        }
        public Task<int> BulkDeleteAsync(DataTable data, Action<ASqlConfig> bulk)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            bulkConfig = ConvertExpression(bulkConfig, data);
            return BulkDeleteAsync(data, bulkConfig, true);
        }
        public int BulkDelete(DataTable data, Action<ASqlConfig> bulk)
        {
            ASqlConfig bulkConfig = new ASqlConfig();
            bulk?.Invoke(bulkConfig);
            bulkConfig = ConvertExpression(bulkConfig, data);
            return BulkDeleteAsync(data, bulkConfig, false).GetAwaiter().GetResult();
        }
        private async Task<int> BulkDeleteAsync(DataTable data, ASqlConfig bulkConfig, bool isAsync)
        {
            if (data.Rows.Count == 0) return 0;
            int result = 0;
            var ConfigTableTemp = isAsync ? await CreateQueryDeleteTemp(bulkConfig, isAsync) : CreateQueryDeleteTemp(bulkConfig, isAsync).GetAwaiter().GetResult();
            try
            {
                var taskQuery = ExecuteScalarAsync(x =>
                {
                    x.SqlQuery = ConfigTableTemp.CreateTableQuery;
                    x.CommandType = CommandType.Text;
                }, isAsync);
                if (isAsync) await taskQuery;
                else taskQuery.GetAwaiter().GetResult();

                using (SqlBulkCopy bulkCopy = GetSqlBulkCopy(bulkConfig, ConfigTableTemp.TableName))
                {
                    try
                    {
                        if (isAsync) await BulkCopyAsync(data, bulkCopy, isAsync);
                        else BulkCopyAsync(data, bulkCopy, isAsync).GetAwaiter().GetResult();
                    }
                    finally
                    {
                        bulkCopy.Close();
                    }
                }
                var taskResult = ExecuteNonQueryAsync(x =>
                {
                    x.SqlQuery = ConfigTableTemp.ExecuteQuery;
                    x.CommandType = CommandType.Text;
                }, isAsync);

                result = isAsync ? await taskResult : taskResult.GetAwaiter().GetResult();
            }
            finally
            {
                var taskDrop = ExecuteScalarAsync(x =>
                {
                    x.SqlQuery = $"drop table {ConfigTableTemp.TableName}";
                    x.CommandType = CommandType.Text;
                }, isAsync);
                if (isAsync) await taskDrop;
                else taskDrop.GetAwaiter().GetResult();
            }
            return result;
        }

        private string GetQueryTable(string tableName)
        {
            var query = @"SELECT 
                            [Table_Name] = tbl.table_schema + '.' + tbl.table_name, 
                            [Column_Name] = col.column_name, 
                            [Column_Type] = col.data_type,
                            [Column_Length] = col.CHARACTER_MAXIMUM_LENGTH,
                            [Column_Prec] =col.NUMERIC_PRECISION,
                            [Column_Scale]=col.NUMERIC_SCALE
                            
                        FROM INFORMATION_SCHEMA.TABLES tbl
                        INNER JOIN INFORMATION_SCHEMA.COLUMNS col 
                            ON col.table_name = tbl.table_name
                            AND col.table_schema = tbl.table_schema

                        WHERE tbl.table_type = 'base table' 
                        and tbl.table_name='{0}'";
            var result = string.Format(query, tableName);
            return result;
        }
        private async Task<BulkConfigQueryTemp> CreateQueryDeleteTemp(ASqlConfig bulk, bool isAsync)
        {
            string tableName = "#temp" + DateTime.Now.Ticks;
            string createTableTemp = $"create table [dbo].[{tableName}] ";
            string querySelect = GetQueryTable(bulk.DestinationTableName);
            var dataR = SqlDataResultAsync<ConfigTable>(x =>
            {
                x.SqlQuery = querySelect;
            }, isAsync);
            var dataResult = (List<ConfigTable>)(isAsync ? await dataR : dataR.GetAwaiter().GetResult());

            createTableTemp += "(";
            foreach (var item in (from x in dataResult
                                  join col in bulk.ColumnInputNames on x.Column_Name.ToLower() equals col.ToLower()
                                  select x))
            {
                createTableTemp += $"[{item.Column_Name}] [{item.Column_Type}]";
                if (item.Column_Type.ToLower().EndsWith("char"))
                {
                    createTableTemp += $" ({item.Column_Length})";
                }
                else if (item.Column_Type.ToLower().EndsWith("decimal"))
                {
                    createTableTemp += $" ({item.Column_Prec},{item.Column_Scale})";
                }

                createTableTemp += ",";
            }
            bulk.DestinationTableName = dataResult.FirstOrDefault().Table_Name;
            createTableTemp = createTableTemp.TrimEnd(',');
            createTableTemp += ")";

            string updateFrom = $"DELETE FROM " + bulk.DestinationTableName;

            updateFrom += $" WHERE EXISTS( SELECT TOP 1 *FROM {tableName} as temp WHERE ";
            foreach (var col in bulk.ColumnPrimaryKeyNames)
            {
                updateFrom += $"[temp].[{col}]=[{col}] AND ";
            }
            updateFrom = updateFrom.Trim().TrimEnd('D').TrimEnd('N').TrimEnd('A') + ")";

            return new BulkConfigQueryTemp
            {
                TableName = tableName,
                CreateTableQuery = createTableTemp,
                ExecuteQuery = updateFrom,
            };
        }

        private async Task<BulkConfigQueryTemp> CreateQueryUpdateTemp(ASqlConfig bulk, bool isAsync)
        {
            string tableName = "#temp" + DateTime.Now.Ticks;
            string createTableTemp = $"create table [dbo].[{tableName}]";
            string updateFrom = $"UPDATE [t] set ";
            string querySelect = GetQueryTable(bulk.DestinationTableName);
            var dataR = SqlDataResultAsync<ConfigTable>(x =>
            {
                x.SqlQuery = querySelect;
            }, isAsync);
            var dataResult = (List<ConfigTable>)(isAsync ? await dataR : dataR.GetAwaiter().GetResult());

            createTableTemp += "(";
            foreach (var item in (from x in dataResult
                                  join col in bulk.ColumnInputNames on x.Column_Name.ToLower() equals col.ToLower()
                                  select x))
            {
                createTableTemp += $"[{item.Column_Name}] [{item.Column_Type}]";
                if (item.Column_Type.ToLower().EndsWith("char"))
                {
                    createTableTemp += $" ({item.Column_Length})";
                }
                else if (item.Column_Type.ToLower().EndsWith("decimal"))
                {
                    createTableTemp += $" ({item.Column_Prec},{item.Column_Scale})";
                }

                createTableTemp += ",";
                if (bulk.ColumnPrimaryKeyNames.Any(x => x.ToLower() == item.Column_Name.ToLower())) continue;
                updateFrom += $"[t].[{item.Column_Name}]=[temp].[{item.Column_Name}],";
            }
            bulk.DestinationTableName = dataResult.FirstOrDefault().Table_Name;
            createTableTemp = createTableTemp.TrimEnd(',');
            createTableTemp += ")";

            updateFrom = updateFrom.TrimEnd(',');
            updateFrom += $" from {bulk.DestinationTableName} as t join {tableName} as temp on ";
            foreach (var col in bulk.ColumnPrimaryKeyNames)
            {
                updateFrom += $"[t].[{col}]=[temp].[{col}] AND ";
            }
            updateFrom = updateFrom.Trim().TrimEnd('D').TrimEnd('N').TrimEnd('A');

            return new BulkConfigQueryTemp
            {
                TableName = tableName,
                CreateTableQuery = createTableTemp,
                ExecuteQuery = updateFrom,
            };
        }
        private SqlBulkCopy GetSqlBulkCopy(ASqlConfig bulkConfig, string TableName)
        {
            SqlBulkCopy bulkCopy = null;
            if (_dbConnection.DbTransaction != null)
            {
                var config = new SqlBulkCopyOptions();
                bulkCopy = new SqlBulkCopy(_dbConnection.DbConnection, config, _dbConnection.DbTransaction);
            }
            else
            {
                bulkCopy = new SqlBulkCopy(_dbConnection.DbConnection);
            }
            bulkCopy.BulkCopyTimeout = bulkConfig.Timeout;
            bulkCopy.DestinationTableName = TableName;
            foreach (var item in bulkConfig.ColumnInputNames) bulkCopy.ColumnMappings.Add(item, item);

            return bulkCopy;
        }
        private Task BulkCopyAsync(DataTable dataTable, SqlBulkCopy bulkCopy, bool isAsync)
        {
            if (isAsync)
            {
                return bulkCopy.WriteToServerAsync(dataTable);
            }
            else
            {
                bulkCopy.WriteToServer(dataTable);
                return Task.CompletedTask;
            }
        }
        private DataTable ToDataTable(IEnumerable data, ASqlConfig bulkConfig)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(data.AsQueryable().ElementType);
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
            {
                var propertyType = prop.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propertyType);
                if (underlyingType != null) propertyType = underlyingType;
                if (propertyType.IsClass)
                {
                    if (typeof(string) != propertyType) continue;
                }
                else if (propertyType.IsConstructedGenericType) continue;
                table.Columns.Add(prop.Name, propertyType);
            }
            if (bulkConfig.ColumnIgnoreInputNames != null)
            {
                foreach (var item in bulkConfig.ColumnIgnoreInputNames)
                {
                    table.Columns.Remove(item);
                }
            }
            ConvertExpression(bulkConfig, table);
            var columna = table.Columns;
            foreach (var item in data)
            {
                DataRow row = table.NewRow();
                foreach (DataColumn prop in columna)
                {
                    var valor = properties[prop.ColumnName].GetValue(item);
                    if (valor != null) row[prop.ColumnName] = valor;
                }
                table.Rows.Add(row);
            }
            return table;
        }
        private ASqlConfig ConvertExpression(ASqlConfig bulkConfig, DataTable dataTable)
        {
            ToInputs(dataTable, bulkConfig);
            return bulkConfig;
        }
        private void ToInputs(DataTable dataTable, ASqlConfig bulkConfig)
        {
            var columNames = new List<string>();
            if (bulkConfig.ColumnInputNames != null && bulkConfig.ColumnInputNames.Any())
            {
                columNames = bulkConfig.ColumnInputNames.ToList();
                if (bulkConfig.ColumnPrimaryKeyNames != null && bulkConfig.ColumnPrimaryKeyNames.Any())
                {
                    foreach (var col in bulkConfig.ColumnPrimaryKeyNames
                    .Where(x =>
                            !columNames.Any(tx => tx.ToLower() == x.ToLower()))) columNames.Add(col);
                }
                var ColumnIgnore = new List<string>();
                if (bulkConfig.ColumnIgnoreInputNames != null) ColumnIgnore.AddRange(bulkConfig.ColumnIgnoreInputNames);
                foreach (DataColumn col in dataTable.Columns)
                {
                    if (!columNames.Any(x => x == col.ColumnName)) ColumnIgnore.Add(col.ColumnName);
                }
                bulkConfig.ColumnIgnoreInputNames = ColumnIgnore.Distinct();
            }
            else
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    columNames.Add(col.ColumnName);
                }
            }
            if (bulkConfig.ColumnIgnoreInputNames != null)
            {
                foreach (var item in bulkConfig.ColumnIgnoreInputNames)
                {
                    columNames.Remove(item);
                }
            }
            bulkConfig.ColumnInputNames = columNames;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
    internal enum ExecuteType
    {
        ExecuteNonQuery,
        ExecuteScalar,
    }
}
