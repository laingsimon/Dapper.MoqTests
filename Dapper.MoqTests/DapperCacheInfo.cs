using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal static class DapperCacheInfo
    {
        private static readonly FieldInfo QueryCacheField = typeof(Dapper.SqlMapper).GetField("_queryCache", BindingFlags.Static | BindingFlags.NonPublic);

        private static IDictionary QueryCache => (IDictionary)QueryCacheField.GetValue(null);

        public static void PurgeQueriedIdentities()
        {
            Dapper.SqlMapper.PurgeQueryCache();
            QueryCache.Clear();
        }

        internal static Dapper.SqlMapper.Identity GetIdentity(MockDbCommand mockDbCommand, IIdentityComparer identityComparer)
        {
            var identities = QueryCache.Keys
                .Cast<Dapper.SqlMapper.Identity>()
                .Where(id => identityComparer.Matches(mockDbCommand, id))
                .ToArray();

            if (identities.Length <= 1)
            {
                return identities.SingleOrDefault()
                    ?? SingleIdentityIfTextMatches(mockDbCommand, identityComparer) 
                    ?? throw GetIdentityNotFoundException(mockDbCommand);
            }

            var ambiguous = identities.Select(id => $"`{id.type?.FullName ?? "<untyped>"}`")
                .OrderBy(id => id)
                .ToArray();

            throw GetIdentityAmbiguousException(mockDbCommand, ambiguous);
        }

        private static Dapper.SqlMapper.Identity SingleIdentityIfTextMatches(MockDbCommand mockDbCommand, IIdentityComparer identityComparer)
        {
            if (QueryCache.Keys.Count != 1)
            {
                return null;
            }

            return QueryCache.Keys
                .Cast<Dapper.SqlMapper.Identity>()
                .Where(id => identityComparer.TextMatches(mockDbCommand, id))
                .SingleOrDefault();
        }

        private static InvalidOperationException GetIdentityNotFoundException(MockDbCommand mockDbCommand)
        {
            return new InvalidOperationException(
                $@"Unable to detect the identity for the command, it could have been one of {QueryCache.Keys.Count} possible options.

command: '{mockDbCommand.CommandText}'
Parameters: `{GetParametersRepresentation(mockDbCommand)}`
CommandType: {mockDbCommand.CommandType}

To be able to Verify the Dapper call accurately the Command and Parameters (and return type) must be unique for every invocation of a Dapper method.");
        }

        private static string GetParametersRepresentation(MockDbCommand mockDbCommand)
        {
            var parameters = mockDbCommand.Parameters.Cast<DbParameter>();
            return string.Join(", ", parameters.Select(p => $"{p.ParameterName} = {p.Value}"));
        }

        private static InvalidOperationException GetIdentityAmbiguousException(MockDbCommand mockDbCommand, IReadOnlyCollection<string> ambiguous)
        {
            var commandType = mockDbCommand.CommandType == 0 
                ? CommandType.Text 
                : mockDbCommand.CommandType;

            return new InvalidOperationException(
                $@"Unable to detect the required response type for the command, it could be one of {ambiguous.Count} possible options.

Command: '{mockDbCommand.CommandText}'
Parameters: `{GetParametersRepresentation(mockDbCommand)}`
CommandType: {commandType}

To be able to Verify the Dapper call accurately the Command and Parameters (and return type) must be unique for every invocation of a Dapper method.

Possible options: {string.Join(", ", ambiguous)}

If this issue cannot be resolved, consider setting `Dapper.MoqTests.Settings.ResetDapperCachePerCommand` to `true`, note this is not a thread-safe approach");
        }
    }
}
