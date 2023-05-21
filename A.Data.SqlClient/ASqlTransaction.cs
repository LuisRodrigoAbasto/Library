using System;
using System.Data;
using System.Data.SqlClient;

namespace A.Data.SqlClient
{
    public class ASqlTransaction : IDisposable
    {
        internal SqlTransaction DbTransaction;
        internal ASqlTransaction(SqlTransaction sqlTransaction)
        {
            DbTransaction = sqlTransaction;
        }
        //
        // Resumen:
        //     Gets the System.Data.SqlClient.SqlConnection object associated with the transaction,
        //     or null if the transaction is no longer valid.
        //
        // Valor:
        //     The System.Data.SqlClient.SqlConnection object associated with the transaction.
        //
        // Comentarios:
        //     ## Remarks A single application may have multiple database connections, each
        //     with zero or more transactions. This property lets you determine the connection
        //     object associated with a particular transaction created by <xref:System.Data.SqlClient.SqlConnection.BeginTransaction%2A>.
        public SqlConnection Connection { get { return DbTransaction.Connection; } }
        //
        // Resumen:
        //     Specifies the System.Data.IsolationLevel for this transaction.
        //
        // Valor:
        //     The System.Data.IsolationLevel for this transaction. The default is ReadCommitted.
        //
        // Comentarios:
        //     ## Remarks Parallel transactions are not supported. Therefore, the <xref:System.Data.IsolationLevel>
        //     applies to the whole transaction. For more information on SQL Server isolation
        //     levels, see [Transaction Isolation Levels](/sql/t-sql/language-elements/transaction-isolation-levels).
        public IsolationLevel IsolationLevel
        {
            get
            {
                return DbTransaction.IsolationLevel;
            }
        }

        //
        // Resumen:
        //     Commits the database transaction.
        //
        // Excepciones:
        //   T:System.Exception:
        //     An error occurred while trying to commit the transaction.
        //
        //   T:System.InvalidOperationException:
        //     The transaction has already been committed or rolled back. -or- The connection
        //     is broken.
        //
        // Comentarios:
        //     ## Remarks The <xref:System.Data.SqlClient.SqlTransaction.Commit%2A> method
        //     is equivalent to the Transact-SQL COMMIT TRANSACTION statement. You cannot roll
        //     back a transaction once it has been committed, because all modifications have
        //     become a permanent part of the database. For more information, see [COMMIT TRANSACTION
        //     (Transact-SQL)](/sql/t-sql/language-elements/commit-transaction-transact-sql).
        //     > [!NOTE] > `Try`/`Catch` exception handling should always be used when committing
        //     or rolling back a <xref:System.Data.SqlClient.SqlTransaction>. Both `Commit`
        //     and <xref:System.Data.SqlClient.SqlTransaction.Rollback%2A> generates an <xref:System.InvalidOperationException>
        //     if the connection is terminated or if the transaction has already been rolled
        //     back on the server. For more information on SQL Server transactions, see [Transactions
        //     (Transact-SQL)](/sql/t-sql/language-elements/transactions-transact-sql). ## Examples
        //     The following example creates a <xref:System.Data.SqlClient.SqlConnection>
        //     and a <xref:System.Data.SqlClient.SqlTransaction>. It also demonstrates how
        //     to use the <xref:System.Data.SqlClient.SqlTransaction.Commit%2A>, <xref:System.Data.SqlClient.SqlConnection.BeginTransaction%2A>,
        //     and <xref:System.Data.SqlClient.SqlTransaction.Rollback%2A> methods. The transaction
        //     is rolled back on any error. `Try`/`Catch` error handling is used to handle any
        //     errors when attempting to commit or roll back the transaction. [!code-csharp[SqlConnection_BeginTransaction
        //     Example#1](~/../sqlclient/doc/samples/SqlConnection_BeginTransaction.cs#1)]
        public void Commit()
        {
            DbTransaction.Commit();
        }
        //
        // Resumen:
        //     Rolls back a transaction from a pending state.
        public void Rollback()
        {
            DbTransaction.Rollback();
        }
        //
        // Resumen:
        //     Rolls back a transaction from a pending state.
        //
        // Excepciones:
        //   T:System.Exception:
        //     An error occurred while trying to commit the transaction.
        //
        //   T:System.InvalidOperationException:
        //     The transaction has already been committed or rolled back. -or- The connection
        //     is broken.
        //
        // Comentarios:
        //     ## Remarks The <xref:System.Data.SqlClient.SqlTransaction.Rollback%2A> method
        //     is equivalent to the Transact-SQL ROLLBACK TRANSACTION statement. For more information,
        //     see [ROLLBACK TRANSACTION (Transact-SQL) ](/sql/t-sql/language-elements/rollback-transaction-transact-sql).
        //     The transaction can only be rolled back from a pending state (after <xref:System.Data.SqlClient.SqlConnection.BeginTransaction%2A>
        //     has been called, but before <xref:System.Data.SqlClient.SqlTransaction.Commit%2A>
        //     is called). The transaction is rolled back in the event it is disposed before
        //     `Commit` or `Rollback` is called. > [!NOTE] > `Try`/`Catch` exception handling
        //     should always be used when rolling back a transaction. A `Rollback` generates
        //     an <xref:System.InvalidOperationException> if the connection is terminated or
        //     if the transaction has already been rolled back on the server. For more information
        //     on SQL Server transactions, see [Transactions (Transact-SQL)](/sql/t-sql/language-elements/transactions-transact-sql).
        //     ## Examples The following example creates a <xref:System.Data.SqlClient.SqlConnection>
        //     and a <xref:System.Data.SqlClient.SqlTransaction>. It also demonstrates how
        //     to use the <xref:System.Data.SqlClient.SqlConnection.BeginTransaction%2A>,
        //     <xref:System.Data.SqlClient.SqlTransaction.Commit%2A>, and <xref:System.Data.SqlClient.SqlTransaction.Rollback%2A>
        //     methods. The transaction is rolled back on any error. `Try`/`Catch` error handling
        //     is used to handle any errors when attempting to commit or roll back the transaction.
        //     [!code-csharp[SqlConnection_BeginTransaction Example#1](~/../sqlclient/doc/samples/SqlConnection_BeginTransaction.cs#1)]
        public void Rollback(string transactionName)
        {
            DbTransaction.Rollback(transactionName);
        }
        //
        // Resumen:
        //     Creates a savepoint in the transaction that can be used to roll back a part of
        //     the transaction, and specifies the savepoint name.
        //
        // Parámetros:
        //   savePointName:
        //     The name of the savepoint.
        //
        // Excepciones:
        //   T:System.Exception:
        //     An error occurred while trying to commit the transaction.
        //
        //   T:System.InvalidOperationException:
        //     The transaction has already been committed or rolled back. -or- The connection
        //     is broken.
        //
        // Comentarios:
        //     ## Remarks <xref:System.Data.SqlClient.SqlTransaction.Save%2A> method is equivalent
        //     to the Transact-SQL SAVE TRANSACTION statement. The value used in the `savePoint`
        //     parameter can be the same value used in the `transactionName` parameter of some
        //     implementations of the <xref:System.Data.SqlClient.SqlConnection.BeginTransaction%2A>
        //     method. Savepoints offer a mechanism to roll back parts of transactions. You
        //     create a savepoint using the <xref:System.Data.SqlClient.SqlTransaction.Save%2A>
        //     method, and then later call the <xref:System.Data.SqlClient.SqlTransaction.Rollback%2A>
        //     method to roll back to the savepoint instead of rolling back to the start of
        //     the transaction.
        public void Save(string savePointName)
        {
            DbTransaction.Save(savePointName);
        }
        //
        // Resumen:
        //     Releases the unmanaged resources used and optionally releases the managed resources.
        //
        // Parámetros:
        //   disposing:
        //     true to release managed and unmanaged resources; false to release only unmanaged
        //     resources.
        //
        // Comentarios:
        //     ## Remarks This method calls <xref:System.Data.Common.DbTransaction.Dispose%2A>.
        public void Dispose()
        {
            DbTransaction.Dispose();
        }
    }
}
