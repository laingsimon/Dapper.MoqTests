namespace Dapper.MoqTests
{
    using System.Data;

    public interface IMockDatabase
    {
        IDataReader Query<T>(string text, object parameters);
        // ReSharper disable UnusedMemberInSuper.Global
        T QuerySingle<T>(string text, object parameters);
        // ReSharper restore UnusedMemberInSuper.Global
        int Execute(string text, object parameters);
    }
}