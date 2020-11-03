using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    public class DapperIdentityComparer : IIdentityComparer
    {
        public bool Matches(DbCommand command, Dapper.SqlMapper.Identity identity)
        {
            return identity.sql == command.CommandText
                      && ParametersMatch(identity, command)
                      && CommandTypeMatches(identity, command);
        }

        public bool TextMatches(DbCommand command, Dapper.SqlMapper.Identity identity)
        {
            return identity.sql == command.CommandText;
        }

        private static bool ParametersMatch(Dapper.SqlMapper.Identity identity, DbCommand mockDbCommand)
        {
            var properties = identity.parametersType?.GetProperties() ?? new PropertyInfo[0];
            return properties.All(p => mockDbCommand.Parameters.Contains(p.Name))
                && properties.Length == mockDbCommand.Parameters.Count;
        }

        private static bool CommandTypeMatches(Dapper.SqlMapper.Identity identity, DbCommand mockDbCommand)
        {
            if (identity.commandType == 0 || identity.commandType == null)
                return true;

            return mockDbCommand.CommandType == identity.commandType.Value;
        }
    }
}
