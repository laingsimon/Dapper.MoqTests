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
        private readonly MockDbParameterCollection _parameters = new MockDbParameterCollection();
        private readonly Lazy<SqlMapper.Identity> _identity;

        public MockDbCommand(MockDatabase database)
        {
            _database = database;
            _identity = new Lazy<SqlMapper.Identity>(() => DapperCacheInfo.GetIdentity(this));
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection => _parameters;

        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel()
        { }

        public override int ExecuteNonQuery()
        {
            var dapperEntrypoint = FirstDapperCallInStack();
            return _database.ExecuteNonQuery(this, false, dapperEntrypoint, _identity.Value.type);
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException("Never executed by Dapper");
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            var dapperEntrypoint = FirstDapperCallInStack();
            return Task.FromResult(_database.ExecuteNonQuery(this, true, dapperEntrypoint, _identity.Value.type));
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Never executed by Dapper");
        }

        public IReadOnlyDictionary<ParameterType, object> GetParameterLookup()
        {
            return new Dictionary<ParameterType, object>
            {
                { ParameterType.SqlText, CommandText },
                { ParameterType.SqlParameters, ParametersObjectBuilder.FromParameters(_parameters) },
                { ParameterType.SqlTransaction, DbTransaction }
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
            var dapperEntrypoint = FirstDapperCallInStack();
            return new MockDbDataReader(_database.ExecuteReader(this, dapperEntrypoint, _identity.Value.type));
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var dapperEntryPoint = FirstDapperCallInStack();
            return Task.FromResult<DbDataReader>(new MockDbDataReader(_database.ExecuteReader(this, dapperEntryPoint, _identity.Value.type)));
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
            if (Settings.ResetDapperCachePerCommand)
                DapperCacheInfo.PurgeQueriedIdentities();

            base.Dispose(disposing);
        }
    }
}