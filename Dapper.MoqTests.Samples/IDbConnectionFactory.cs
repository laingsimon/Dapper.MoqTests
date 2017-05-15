using System.Data;

namespace Dapper.MoqTests.Samples
{
    public interface IDbConnectionFactory
    {
        IDbConnection OpenConnection();
    }
}
