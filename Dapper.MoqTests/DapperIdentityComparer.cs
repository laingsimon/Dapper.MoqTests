using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dapper.MoqTests
{
    public class DapperIdentityComparer : IIdentityComparer
    {
        private readonly IDapperCommandTextHelper commandTextHelper;

        public DapperIdentityComparer(IDapperCommandTextHelper commandTextHelper)
        {
            this.commandTextHelper = commandTextHelper;
        }

        public bool Matches(DbCommand command, Dapper.SqlMapper.Identity identity, CacheInfoProxy cacheInfo = null)
        {
            return TextMatches(command, identity, cacheInfo)
                      && ParametersMatch(identity, command)
                      && CommandTypeMatches(identity, command);
        }

        public bool TextMatches(DbCommand command, Dapper.SqlMapper.Identity identity, CacheInfoProxy cacheInfo = null)
        {
            var commandText = command.CommandText;
            if (commandText.Contains("@") && identity.sql.Contains("@"))
            {
                commandText = commandTextHelper.ConvertDapperParametersToUserParameters(commandText);
            }

            return identity.sql == commandText;
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
