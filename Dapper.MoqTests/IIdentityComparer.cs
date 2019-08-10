using System.Data.Common;

namespace Dapper.MoqTests
{
    public interface IIdentityComparer
    {
        bool Matches(DbCommand command, SqlMapper.Identity identity);
        bool TextMatches(DbCommand command, SqlMapper.Identity identity);
    }
}
