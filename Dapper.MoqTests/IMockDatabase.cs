namespace Dapper.MoqTests
{
    using System.Collections.Generic;

    public interface IMockDatabase
    {
        T QuerySingle<T>(string text, object parameters = null);
        IEnumerable<T> Query<T>(string text, object parameters = null);
        int Execute(string text, object parameters = null);
    }
}