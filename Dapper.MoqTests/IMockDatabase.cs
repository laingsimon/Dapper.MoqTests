using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    using System.Collections.Generic;

    public interface IMockDatabase
    {
        T QuerySingle<T>([ParameterType(ParameterType.SqlText)] string text);
        IEnumerable<T> Query<T>([ParameterType(ParameterType.SqlText)] string text);
        int Execute([ParameterType(ParameterType.SqlText)] string text);
        Task<IEnumerable<T>> QueryAsync<T>([ParameterType(ParameterType.SqlText)] string text);
        Task<T> QuerySingleAsync<T>([ParameterType(ParameterType.SqlText)] string text);
        Task<int> ExecuteAsync([ParameterType(ParameterType.SqlText)] string text);

        T QuerySingle<T>([ParameterType(ParameterType.SqlText)] string text, [ParameterType(ParameterType.SqlParameters)] object parameters);
        IEnumerable<T> Query<T>([ParameterType(ParameterType.SqlText)] string text, [ParameterType(ParameterType.SqlParameters)] object parameters);
        int Execute([ParameterType(ParameterType.SqlText)] string text, [ParameterType(ParameterType.SqlParameters)] object parameters);
        Task<IEnumerable<T>> QueryAsync<T>([ParameterType(ParameterType.SqlText)] string text, [ParameterType(ParameterType.SqlParameters)] object parameters);
        Task<T> QuerySingleAsync<T>([ParameterType(ParameterType.SqlText)] string text, [ParameterType(ParameterType.SqlParameters)] object parameters);
        Task<int> ExecuteAsync([ParameterType(ParameterType.SqlText)] string text, [ParameterType(ParameterType.SqlParameters)] object parameters);
    }
}