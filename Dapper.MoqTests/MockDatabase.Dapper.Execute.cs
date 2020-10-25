using System.Data;
using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    public abstract partial class MockDatabase
    {
        /// <summary>
        /// Execute parameterized SQL.
        /// </summary>
        public abstract int Execute(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a command asynchronously using .NET 4.5 Task.
        /// </summary>
        public abstract Task<int> ExecuteAsync(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute parameterized SQL.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute on this connection.</param>
        /// <returns>The number of rows affected.</returns>
        public abstract int Execute(
            [ParameterType(ParameterType.CommandDefinition)] CommandDefinition command);

        /// <summary>
        /// Execute a command asynchronously using Task.
        /// </summary>
        /// <param name="cnn">The connection to execute on.</param>
        /// <param name="command">The command to execute on this connection.</param>
        /// <returns>The number of rows affected.</returns>
        public abstract Task<int> ExecuteAsync(
            [ParameterType(ParameterType.CommandDefinition)] CommandDefinition command);
    }
}
