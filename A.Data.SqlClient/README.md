#  A.Data.SqlClient

## Why choose A.Data.SqlClient?

### Free

Abasto SqlClient are open source and free for commercial use. You can install them from [NuGet](https://www.nuget.org/packages/A.Data.SqlClient)

### Examples
```cs
{

using (var sqlClient = new ASqlConnection(_configuration.GetConnectionString("Connection")))
            {
                await sqlClient.OpenAsync();
                var data = sqlClient.SqlData(x =>
                {
                    x.SqlQuery = "select top 1000 *from dbo.Employe";
                });

                using (var transaction = sqlClient.BeginTransaction())
                {
                    var count = await sqlClient.BulkInsertAsync(data, x =>
                    {
                        x.DestinationTableName = "dbo.Employe2";
                        x.ColumnIgnoreInputNames = new[] { "empId" };
                    });
                    transaction.Commit();
                }
            }
}
```