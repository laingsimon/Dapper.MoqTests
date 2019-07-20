using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal static class DapperCacheInfo
    {
        private static readonly FieldInfo QueryCacheField = typeof(SqlMapper).GetField("_queryCache", BindingFlags.Static | BindingFlags.NonPublic);

        private static IDictionary QueryCache => (IDictionary)QueryCacheField.GetValue(null);

        public static void PurgeQueriedIdentities()
        {
            SqlMapper.PurgeQueryCache();
            QueryCache.Clear();
        }

        internal static SqlMapper.Identity GetIdentity(MockDbCommand mockDbCommand)
        {
            var identities = (from identity in QueryCache.Keys.Cast<SqlMapper.Identity>()
                where identity.sql == mockDbCommand.CommandText
                      && ParametersMatch(identity, mockDbCommand)
                select identity).ToArray();

            if (identities.Length <= 1)
                return identities.SingleOrDefault();

            var ambiguous =
                identities.Select(id => $"`{id.type?.FullName ?? "<untyped>"}`");

            throw new InvalidOperationException(
                $@"Unable to detect the required response type for the command, it could be one of {identities.Length} possible options.

Command: '{mockDbCommand.CommandText}'
Parameters: `{mockDbCommand.Parameters}`
CommandType: {mockDbCommand.CommandType}

To be able to Verify the Dapper call accurately the Command and Parameters (and return type) must be unique for every invocation of a Dapper method.

Possible options: {string.Join(", ", ambiguous)}

If this issue cannot be resolved, consider setting `Dapper.MoqTests.Settings.ResetDapperCachePerCommand` to `true`, note this is not a thread-safe approach");
        }

        private static bool ParametersMatch(SqlMapper.Identity identity, MockDbCommand mockDbCommand)
        {
            var properties = identity.parametersType?.GetProperties() ?? new PropertyInfo[0];
            return properties.All(p => mockDbCommand.Parameters.Contains(p.Name))
                && properties.Length == mockDbCommand.Parameters.Count;
        }
    }
}
