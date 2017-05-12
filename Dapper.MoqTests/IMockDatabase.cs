namespace Dapper.MoqTests
{
    using System.Data;

    public interface IMockDatabase
    {
        IDataReader Query(string text, object parameters);
        // ReSharper disable UnusedMemberInSuper.Global
        object QuerySingle(string text, object parameters);
        // ReSharper restore UnusedMemberInSuper.Global
        int Execute(string text, object parameters);
    }
}