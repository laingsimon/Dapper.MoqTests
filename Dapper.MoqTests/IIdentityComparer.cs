using System.Data.Common;

namespace Dapper.MoqTests
{
    public interface IIdentityComparer
    {
        bool Matches(DbCommand command, Dapper.SqlMapper.Identity identity);
        bool TextMatches(DbCommand command, Dapper.SqlMapper.Identity identity);
    }
}
