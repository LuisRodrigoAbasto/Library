using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace A.Data.SqlClientCore.Model
{
    internal class BulkConfigQueryTemp
    {
        public string TableName { get; set; }
        public string CreateTableQuery { get; set; }
        public string ExecuteQuery { get; set; }
    }
    internal class ConfigTable
    {
        public string Table_Name { get; set; }
        public string Column_Name { get; set; }
        public string Column_Type { get; set; }
        public int? Column_Length { get; set; }
        public byte? Column_Prec { get; set; }
        public int? Column_Scale { get; set; }
    }
    public class BulkConfig
    {
        public int Timeout { get; set; } = 30;
        public string DestinationTableName { get; set; }
        public IEnumerable<string> ColumnPrimaryKeyNames { get; set; }
    }
    public class ASqlConfig : BulkConfig
    {
        //
        // Summary:
        //     Gets or sets the name of the destination schema.
        //
        // Value:
        //     The name of the destination schema.
        // public string DestinationSchemaName { get; set; }
        //
        // Summary:
        //     Gets or sets the name of the destination table.
        //
        // Value:
        //     The name of the destination table.
        //
        // Summary:
        //     Gets or sets a list of property names of primary key.
        //
        // Summary:
        //     Gets or sets the column primary key expression.
        //
        // Value:
        //     The column primary key expression.
        // public Expression<Func<object, object>> ColumnPrimaryKeyExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the column input expression.
        //
        // Value:
        //     The column input expression.
        // public Expression<Func<object, object>> ColumnInputExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the column input/output expression.
        //
        // Value:
        //     The column input/output expression.
        // public Expression<Func<object, object>> ColumnInputOutputExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the column output expression.
        //
        // Value:
        //     The column output expression.
        // public Expression<Func<object, object>> ColumnOutputExpression { get; set; }
        //
        // Summary:
        //     Gets or sets a list of property names to input during the Merge Update statement.
        // public List<string> OnMergeUpdateInputNames { get; set; }
        //
        // Summary:
        //     Gets or sets the column synchronize delete key subset expression.
        //
        // Value:
        //     The column synchronize delete key subset expression.
        // public Expression<Func<object, object>> ColumnSynchronizeDeleteKeySubsetExpression { get; set; }
        //
        // Summary:
        //     Gets or sets columns output to ignore using an expression.
        //
        // Value:
        //     The columns output to ignore using an expression.
        // public Expression<Func<object, object>> IgnoreColumnOutputExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the ignore on insert expression.
        //
        // Value:
        //     The ignore on insert expression.
        // public Expression<Func<object, object>> IgnoreOnInsertExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the ignore on update expression.
        //
        // Value:
        //     The ignore on update expression.
        public Expression<Func<object, object>> IgnoreOnUpdateExpression { get; set; }
        //
        // Summary:
        //     Gets or sets a list of property names to input.
        public IEnumerable<string> ColumnInputNames { get; set; }
        public IEnumerable<string> ColumnIgnoreInputNames { get; set; }
        //
        // Summary:
        //     Gets or sets a list of property names to input/output.
        // public List<string> ColumnInputOutputNames { get; set; }
        //
        // Summary:
        //     Gets or sets a list of property names to output.
        // public List<string> ColumnOutputNames { get; set; }
        // public void Dispose() { }
    }
    public class BulkConfig<T> : ASqlConfig where T : class
    {
        public BulkConfig() { }
        public Expression<Func<T, object>> ColumnPrimaryKeyExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the column input expression.
        //
        // Value:
        //     The column input expression.
        public Expression<Func<T, object>> ColumnInputExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the column input/output expression.
        //
        // Value:
        //     The column input/output expression.
        public Expression<Func<T, object>> ColumnInputOutputExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the column output expression.
        //
        // Value:
        //     The column output expression.
        public Expression<Func<T, object>> ColumnOutputExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the column synchronize delete key subset expression.
        //
        // Value:
        //     The column synchronize delete key subset expression.
        public Expression<Func<T, object>> ColumnSynchronizeDeleteKeySubsetExpression { get; set; }
        //
        // Summary:
        //     Gets or sets columns output to ignore using an expression.
        //
        // Value:
        //     The columns output to ignore using an expression.
        public Expression<Func<T, object>> IgnoreColumnOutputExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the ignore on insert expression.
        //
        // Value:
        //     The ignore on insert expression.
        public Expression<Func<T, object>> IgnoreOnInsertExpression { get; set; }
        //
        // Summary:
        //     Gets or sets the ignore on update expression.
        //
        // Value:
        //     The ignore on update expression.
        public Expression<Func<T, object>> IgnoreOnUpdateExpression { get; set; }
    }
    public class BulkConnection<TSqlConection>
    {
        public BulkConnection() { }
        public TSqlConection SqlConnection { get; set; }
    }
    public class BulkConnection<TSqlConection, TSqlTransaction> : BulkConnection<TSqlConection>
    {
        public BulkConnection() { }
        public TSqlTransaction SqlTransaction { get; set; }
    }
}
