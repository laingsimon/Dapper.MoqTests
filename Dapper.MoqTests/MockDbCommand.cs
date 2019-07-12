using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MockDbCommand : DbCommand
    {
        private readonly MockDatabase database;
        private readonly MockDbParameterCollection parameters = new MockDbParameterCollection();

        public MockDbCommand(MockDatabase database)
        {
            this.database = database;
        }

        public override string CommandText { get; set; }
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection DbConnection { get; set; }

        protected override DbParameterCollection DbParameterCollection => parameters;

        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel()
        { }

        public override int ExecuteNonQuery()
        {
            var dapperEntrypoint = FirstDapperCallInStack();
            return database.ExecuteNonQuery(this, false, dapperEntrypoint);
        }

        public override object ExecuteScalar()
        {
            var dapperEntrypoint = FirstDapperCallInStack();
            return database.ExecuteScalar(this, dapperEntrypoint);
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            var dapperEntrypoint = FirstDapperCallInStack();
            return Task.FromResult(database.ExecuteNonQuery(this, true, dapperEntrypoint));
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var dapperEntrypoint = FirstDapperCallInStack();
            return Task.FromResult(database.ExecuteScalar(this, dapperEntrypoint));
        }

        public IReadOnlyDictionary<ParameterType, object> GetParameterLookup()
        {
            return new Dictionary<ParameterType, object>
            {
                { ParameterType.SqlText, CommandText },
                { ParameterType.SqlParameters, ParametersObjectBuilder.FromParameters(parameters) },
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
            return new MockDbDataReader(database.ExecuteReader(this, dapperEntrypoint));
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var dapperEntryPoint = FirstDapperCallInStack();
            return Task.FromResult<DbDataReader>(new MockDbDataReader(database.ExecuteReader(this, dapperEntryPoint)));
        }

        private MethodBase FirstDapperCallInStack()
        {
            //This is inefficient, but yields accurate results, so is worth the performance hit

            var stack = new StackTrace();
            return stack.GetFrames()?
                .Select(f => f.GetMethod())
                .LastOrDefault(method => method.DeclaringType == typeof(SqlMapper));
        }
    }
}