using System;
using System.Data;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Dapper.MoqTests
{
    public abstract partial class MockDatabase
    {
        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract GridReader QueryMultiple(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<GridReader> QueryMultipleAsync(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);
    }
}
