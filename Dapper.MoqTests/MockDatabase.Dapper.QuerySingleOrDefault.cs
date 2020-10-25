using System;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    public abstract partial class MockDatabase
    {
        /// <summary>
        /// Executes a single-row query, returning the data typed as T.
        /// </summary>
        public abstract T QuerySingleOrDefault<T>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Return a dynamic object with properties matching the columns.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract dynamic QuerySingleOrDefault(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Executes a single-row query, returning the data typed as type.
        /// </summary>
        public abstract object QuerySingleOrDefault(
            [ParameterType(ParameterType.Type)] Type type,
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a single-row query asynchronously using .NET 4.5 Task.
        /// </summary>
        public abstract Task<object> QuerySingleOrDefaultAsync(
            [ParameterType(ParameterType.Type)] Type type,
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a single-row query asynchronously using .NET 4.5 Task.
        /// </summary>
        public abstract Task<T> QuerySingleOrDefaultAsync<T>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a single-row query asynchronously using .NET 4.5 Task.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<dynamic> QuerySingleOrDefaultAsync(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="command">The command used to query on this connection.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public abstract Task<T> QuerySingleOrDefaultAsync<T>(
            [ParameterType(ParameterType.CommandDefinition)] CommandDefinition command);

        /// <summary>
        /// Execute a single-row query asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to query on.</param>
        /// <param name="command">The command used to query on this connection.</param>
        /// <remarks>Note: the row can be accessed via "dynamic", or by casting to an IDictionary&lt;string,object&gt;</remarks>
        public abstract Task<T> QuerySingleOrDefaultAsync<T>(
            [ParameterType(ParameterType.Type)] Type type,
            [ParameterType(ParameterType.CommandDefinition)] CommandDefinition command);
    }
}
