using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    [System.ComponentModel.DesignerCategory("Code")]
    internal class MockDbCommand : DbCommand
    {
        private readonly MockDatabase _database;
        private readonly Settings _settings;
        private readonly Lazy<Dapper.SqlMapper.Identity> _identity;

        public MockDbCommand(MockDatabase database, Settings settings)
        {
            _database = database;
            _settings = settings;
            _identity = new Lazy<Dapper.SqlMapper.Identity>(() => DapperCacheInfo.GetIdentity(this, settings.IdentityComparer));

            DbParameterCollection = new MockDbParameterCollection(settings);
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }
        protected override DbParameterCollection DbParameterCollection { get; }

        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel()
        { }

        public override int ExecuteNonQuery()
        {
            var dapperMethod = FirstDapperCallInStack();
            return _database.ExecuteNonQuery(this, false, CancellationToken.None, dapperMethod, _identity.Value.type);
        }

        public override object ExecuteScalar()
        {
            var dapperMethod = FirstDapperCallInStack();
            return _database.ExecuteScalar(this, false, CancellationToken.None, dapperMethod);
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            var dapperMethod = FirstDapperCallInStack();
            return Task.FromResult(_database.ExecuteNonQuery(this, true, cancellationToken, dapperMethod, _identity.Value.type));
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var dapperMethod = FirstDapperCallInStack();
            return Task.FromResult(_database.ExecuteScalar(this, true, cancellationToken, dapperMethod));
        }

        public IReadOnlyDictionary<ParameterType, object> GetParameterLookup(bool async, CancellationToken cancellationToken)
        {
            var parameters = DbParameterCollection.Cast<DbParameter>().ToArray();

            var lookup = new Dictionary<ParameterType, object>
            {
                { ParameterType.Buffered, GetBufferedParameter(async) },
                { ParameterType.CommandTimeout, CommandTimeout == 0 ? default(int?) : CommandTimeout },
                { ParameterType.CommandType, CommandType == 0 ? default(CommandType?) : CommandType },
                { ParameterType.Map, GetMap() },
                { ParameterType.SplitOn, GetSplitOn() },
                { ParameterType.SqlParameters, _settings.SqlParametersBuilder.FromParameters(parameters) },
                { ParameterType.SqlText, CommandText },
                { ParameterType.SqlTransaction, DbTransaction },
                { ParameterType.Type, _identity.Value.type },
                { ParameterType.Types, GetDataTypes() }
            };

            lookup[ParameterType.CommandDefinition] = new CommandDefinition(
                (string)lookup[ParameterType.SqlText],
                lookup[ParameterType.SqlParameters],
                (IDbTransaction)lookup[ParameterType.SqlTransaction],
                (int?)lookup[ParameterType.CommandTimeout],
                (CommandType?)lookup[ParameterType.CommandType],
                GetBufferedParameter(async)
                    ? CommandFlags.Buffered
                    : CommandFlags.None,
                cancellationToken
            );

            return lookup;
        }

        private string GetSplitOn()
        {
            return _settings.Unresolved.SplitOn;
        }

        private object GetMap()
        {
            return null;
        }

        private Type[] GetDataTypes()
        {
            var identityInstance = _identity.Value;
            var genericTypes = identityInstance.GetType().GetGenericArguments();

            if (genericTypes.Any())
                return genericTypes;

            return null;
        }

        private bool GetBufferedParameter(bool isAsync)
        {
            if (isAsync)
                return true;

            return _settings.Unresolved.Buffered;
        }

        public override void Prepare()
        { }

        protected override DbParameter CreateDbParameter()
        {
            return new MockDbParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var dapperMethod = FirstDapperCallInStack();
            return new MockDbDataReader(_database.ExecuteReader(this, false, CancellationToken.None, dapperMethod, GetDataTypes() ?? new[] { _identity.Value.type }));
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var dapperMethod = FirstDapperCallInStack();
            return Task.FromResult<DbDataReader>(new MockDbDataReader(_database.ExecuteReader(this, true, cancellationToken, dapperMethod, GetDataTypes() ?? new[] { _identity.Value.type })));
        }

        private MethodBase FirstDapperCallInStack()
        {
            //This is inefficient, but yields accurate results, so is worth the performance hit

            var stack = new StackTrace();
            return stack.GetFrames()?
                .Select(f => f.GetMethod())
                .LastOrDefault(method => method.DeclaringType == typeof(Dapper.SqlMapper));
        }

        protected override void Dispose(bool disposing)
        {
            if (_settings.ResetDapperCachePerCommand)
                DapperCacheInfo.PurgeQueriedIdentities();

            base.Dispose(disposing);
        }
    }
}