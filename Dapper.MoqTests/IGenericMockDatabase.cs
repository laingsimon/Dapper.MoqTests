namespace Dapper.MoqTests
{
    using System.Data;

    internal interface IGenericMockDatabase
    {
        IDataReader Query(string text, object parameters);
        object QuerySingle(string text, object parameters);
    }
}