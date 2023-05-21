using A.Data.SqlClient.Model;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace A.Data.SqlClient
{
    public sealed class ASqlConnection : ADbConnection<SqlConnection, SqlTransaction>, IASqlConnection
    {
        internal ASqlConnection(ASqlConnection aSqlConnection)
        {
            DbConnection = aSqlConnection.DbConnection;
            DbTransaction = aSqlConnection.DbTransaction;
        }
        public ASqlConnection(string connectionString)
        {
            DbConnection = new SqlConnection(connectionString);
        }
        public ASqlConnection(SqlConnection sqlConnection)
        {
            DbConnection = sqlConnection;
        }
        public ASqlConnection(SqlConnection sqlConnection, SqlTransaction sqlTransaction)
        {
            DbConnection = sqlConnection;
            DbTransaction = sqlTransaction;
        }
        public SqlConnection GetDbConnection()
        { return DbConnection; }
        public SqlTransaction GetDbTransaction()
        { return DbTransaction; }
        public new ASqlTransaction BeginTransaction()
        {
            DbTransaction = DbConnection.BeginTransaction();
            return new ASqlTransaction(DbTransaction);
        }
        public override void Open()
        {
            OpenAsync(false).GetAwaiter().GetResult();
        }
        public override Task OpenAsync()
        {
            return OpenAsync(true);
        }
        private Task OpenAsync(bool isAsync)
        {
            if (isAsync) return DbConnection.OpenAsync();
            DbConnection.Open();
            return Task.CompletedTask;
        }
        public override void Close()
        {
            DbConnection.Close();
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

    }

}
