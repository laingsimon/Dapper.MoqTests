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
            return database.ExecuteNonQuery(this, false);
        }

        public override object ExecuteScalar()
        {
            return database.ExecuteScalar(this, false);
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(database.ExecuteNonQuery(this, true));
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(database.ExecuteScalar(this, true));
        }

        public IReadOnlyDictionary<string, object> GetParameterLookup()
        {
            return new Dictionary<string, object>
            {
                { "text", CommandText },
                { "parameters", ParametersObjectBuilder.FromParameters(parameters) }
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
            return new MockDbDataReader(database.ExecuteReader(this, false, RequiresSingleResult(behavior)));
        }

        protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            return Task.FromResult<DbDataReader>(new MockDbDataReader(database.ExecuteReader(this, true, RequiresSingleResult(behavior))));
        }

        private bool? RequiresSingleResult(CommandBehavior behaviour)
        {
            if (!behaviour.HasFlag(CommandBehavior.SingleResult))
                return null; //something hasn't been passed in the way Dapper codebase says it would

            //This flag never appears to be passed across, but Dapper codebase says it should
            return behaviour.HasFlag(CommandBehavior.SingleRow);
        }
    }
}