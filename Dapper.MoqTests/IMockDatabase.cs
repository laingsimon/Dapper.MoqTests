namespace Dapper.MoqTests
{
    using System.Data;

    public interface IMockDatabase
    {
        IDataReader Query<T>(string text, object parameters = null);
        // ReSharper disable UnusedMemberInSuper.Global
        T QuerySingle<T>(string text, object parameters = null);
        // ReSharper restore UnusedMemberInSuper.Global
        int Execute(string text, object parameters = null);
    }
}