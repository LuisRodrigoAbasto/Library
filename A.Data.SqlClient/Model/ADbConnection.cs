using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace A.Data.SqlClient.Model
{
    public abstract class ADbConnection : IDisposable
    {
        public ADbConnection()
        { }
        public abstract string ConnectionString { get; }
        public abstract DbConnection Connection { get; }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) Connection.Close();
        }

    }
    public abstract class ADbConnection<TDbConnection> : ADbConnection
    {
        public ADbConnection() { }
        public override string ConnectionString
        {
            get
            {
                return Connection.ConnectionString;
            }
        }
        public override DbConnection Connection
        {
            get
            {
                return this.DbConnection as DbConnection;
            }
        }
        internal TDbConnection DbConnection { get; set; }
    }
    public abstract class ADbConnection<TDbConnection, TDbTransaction> : ADbConnection<TDbConnection>
    {
        internal TDbTransaction DbTransaction { get; set; }
        public abstract void Open();
        public abstract Task OpenAsync();
        public abstract void Close();
        public TDbTransaction BeginTransaction()
        {
            throw null;
        }
    }

}
