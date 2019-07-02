using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    using System.Collections.Generic;

    public interface IMockDatabase
    {
        T QuerySingle<T>(string text, object parameters = null);
        IEnumerable<T> Query<T>(string text, object parameters = null);
        int Execute(string text, object parameters = null);
        Task<IEnumerable<T>> QueryAsync<T>(string text, object parameters = null);
        Task<T> QuerySingleAsync<T>(string text, object parameters = null);
        Task<int> ExecuteAsync(string text, object parameters = null);
    }
}