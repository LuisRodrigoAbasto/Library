using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace A.Data.SqlClient
{
    public interface IASqlConnection : IDisposable
    {
        ASqlTransaction BeginTransaction();

        SqlConnection GetDbConnection();

        SqlTransaction GetDbTransaction();
        void Close();

        void Open();

        Task OpenAsync();
    }
}