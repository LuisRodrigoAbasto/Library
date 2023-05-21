using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace A.Data.SqlClientCore
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