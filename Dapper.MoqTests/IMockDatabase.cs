using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    using System.Collections.Generic;
    using System.Data;

    public interface IMockDatabase
    {
        T QuerySingle<T>(
            [ParameterType(ParameterType.SqlText)] string text,
            [ParameterType(ParameterType.SqlParameters)] object parameters = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null);

        IEnumerable<T> Query<T>(
            [ParameterType(ParameterType.SqlText)] string text,
            [ParameterType(ParameterType.SqlParameters)] object parameters = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null);

        int Execute(
            [ParameterType(ParameterType.SqlText)] string text,
            [ParameterType(ParameterType.SqlParameters)] object parameters = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null);

        Task<IEnumerable<T>> QueryAsync<T>(
            [ParameterType(ParameterType.SqlText)] string text,
            [ParameterType(ParameterType.SqlParameters)] object parameters = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null);

        Task<T> QuerySingleAsync<T>(
            [ParameterType(ParameterType.SqlText)] string text,
            [ParameterType(ParameterType.SqlParameters)] object parameters = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null);

        Task<int> ExecuteAsync(
            [ParameterType(ParameterType.SqlText)] string text,
            [ParameterType(ParameterType.SqlParameters)] object parameters = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null);
    }
}