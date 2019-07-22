using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
                      && CommandTypeMatches(identity, mockDbCommand)
                select identity).ToArray();

            if (identities.Length <= 1)
            {
                return identities.SingleOrDefault() 
                    ?? SingleIdentityIfTextMatches(mockDbCommand) 
                    ?? throw GetIdentityNotFoundException(mockDbCommand);
            }

            var ambiguous =
                identities.Select(id => $"`{id.type?.FullName ?? "<untyped>"}`")
                .ToArray();

            throw GetIdentityAmbiguousException(mockDbCommand, ambiguous);
        }

        private static SqlMapper.Identity SingleIdentityIfTextMatches(MockDbCommand mockDbCommand)
        {
            if (QueryCache.Keys.Count != 1)
            {
                return null;
            }

            return (from identity in QueryCache.Keys.Cast<SqlMapper.Identity>()
                    where identity.sql == mockDbCommand.CommandText
                    select identity).SingleOrDefault();
        }

        private static InvalidOperationException GetIdentityNotFoundException(MockDbCommand mockDbCommand)
        {
            return new InvalidOperationException(
                $@"Unable to detext the identity for the command, it could have been one of {QueryCache.Keys.Count} possible options.

command: '{mockDbCommand.CommandText}'
Parameters: `{mockDbCommand.Parameters}`
CommandType: {mockDbCommand.CommandType}

To be able to Verify the Dapper call accurately the Command and Parameters (and return type) must be unique for every invocation of a Dapper method.");
        }

        private static InvalidOperationException GetIdentityAmbiguousException(MockDbCommand mockDbCommand, IReadOnlyCollection<string> ambiguous)
        {
            var commandType = mockDbCommand.CommandType == 0 
                ? CommandType.Text 
                : mockDbCommand.CommandType;

            return new InvalidOperationException(
                $@"Unable to detect the required response type for the command, it could be one of {ambiguous.Count} possible options.

Command: '{mockDbCommand.CommandText}'
Parameters: `{mockDbCommand.Parameters}`
CommandType: {commandType}

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

        private static bool CommandTypeMatches(SqlMapper.Identity identity, MockDbCommand mockDbCommand)
        {
            if (identity.commandType == 0 || identity.commandType == null)
                return true;

            return mockDbCommand.CommandType == identity.commandType.Value;
        }
    }
}
