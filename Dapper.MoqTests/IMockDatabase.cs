using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    using System.Collections.Generic;

    public interface IMockDatabase
    {
        T QuerySingle<T>(string text);
        IEnumerable<T> Query<T>(string text);
        int Execute(string text);
        Task<IEnumerable<T>> QueryAsync<T>(string text);
        Task<T> QuerySingleAsync<T>(string text);
        Task<int> ExecuteAsync(string text);

        T QuerySingle<T>(string text, object parameters);
        IEnumerable<T> Query<T>(string text, object parameters);
        int Execute(string text, object parameters);
        Task<IEnumerable<T>> QueryAsync<T>(string text, object parameters);
        Task<T> QuerySingleAsync<T>(string text, object parameters);
        Task<int> ExecuteAsync(string text, object parameters);
    }
}