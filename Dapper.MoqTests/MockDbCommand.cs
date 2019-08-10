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
    internal class MockDbCommand : DbCommand
    {
        private readonly MockDatabase _database;
        private readonly Settings _settings;
        private readonly Lazy<SqlMapper.Identity> _identity;

        public MockDbCommand(MockDatabase database, Settings settings)
        {
            _database = database;
            _settings = settings;
            _identity = new Lazy<SqlMapper.Identity>(() => DapperCacheInfo.GetIdentity(this));

            DbParameterCollection = new MockDbParameterCollection();
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
            return _database.ExecuteNonQuery(this, false, dapperMethod, _identity.Value.type);
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException("Never executed by Dapper");
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            var dapperMethod = FirstDapperCallInStack();
            return Task.FromResult(_database.ExecuteNonQuery(this, true, dapperMethod, _identity.Value.type));
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Never executed by Dapper");
        }

        public IReadOnlyDictionary<ParameterType, object> GetParameterLookup()
        {
            var parameters = DbParameterCollection.Cast<DbParameter>().ToArray();

            return new Dictionary<ParameterType, object>
            {
                { ParameterType.Buffered, true }, //TODO: Work out this value
                { ParameterType.CommandTimeout, CommandTimeout == 0 ? default(int?) : CommandTimeout },
                { ParameterType.CommandType, CommandType == 0 ? default(CommandType?) : CommandType },
                { ParameterType.Map, null }, //TODO: Probably cannot access this value
                { ParameterType.SplitOn, null }, //TODO: Probably cannot access this value
                { ParameterType.SqlParameters, _settings.SqlParametersBuilder.FromParameters(parameters) },
                { ParameterType.SqlText, CommandText },
                { ParameterType.SqlTransaction, DbTransaction },
                { ParameterType.Type, _identity.Value.type },
                { ParameterType.Types, null } //TODO: Probably cannot access this value
            };
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
            return new MockDbDataReader(_database.ExecuteReader(this, dapperMethod, _identity.Value.type));
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var dapperMethod = FirstDapperCallInStack();
            return Task.FromResult<DbDataReader>(new MockDbDataReader(_database.ExecuteReader(this, dapperMethod, _identity.Value.type)));
        }

        private MethodBase FirstDapperCallInStack()
        {
            //This is inefficient, but yields accurate results, so is worth the performance hit

            var stack = new StackTrace();
            return stack.GetFrames()?
                .Select(f => f.GetMethod())
                .LastOrDefault(method => method.DeclaringType == typeof(SqlMapper));
        }

        protected override void Dispose(bool disposing)
        {
            if (_settings.ResetDapperCachePerCommand)
                DapperCacheInfo.PurgeQueriedIdentities();

            base.Dispose(disposing);
        }
    }
}