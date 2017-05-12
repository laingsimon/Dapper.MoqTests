namespace Dapper.MoqTests
{
    using System;
    using System.Data;

    internal class MockDbCommand : IDbCommand
    {
        private readonly MockDatabase database;
        private readonly MockDbParameterCollection parameters = new MockDbParameterCollection();

        public MockDbCommand(MockDatabase database)
        {
            this.database = database;
        }

        void IDisposable.Dispose()
        { }

        void IDbCommand.Prepare()
        { }

        void IDbCommand.Cancel()
        { }

        IDbDataParameter IDbCommand.CreateParameter()
        {
            return new MockDbParameter();
        }

        int IDbCommand.ExecuteNonQuery()
        {
            return database.ExecuteNonQuery(new SqlText(CommandText), parameters);
        }

        IDataReader IDbCommand.ExecuteReader()
        {
            return database.ExecuteReader(new SqlText(CommandText), parameters);
        }

        IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
        {
            return database.ExecuteReader(new SqlText(CommandText), parameters);
        }

        object IDbCommand.ExecuteScalar()
        {
            return database.ExecuteScalar(new SqlText(CommandText), parameters);
        }

        IDataParameterCollection IDbCommand.Parameters => parameters;
        IDbConnection IDbCommand.Connection { get; set; }
        IDbTransaction IDbCommand.Transaction { get; set; }
        public string CommandText { get; set; }
        int IDbCommand.CommandTimeout { get; set; }
        CommandType IDbCommand.CommandType { get; set; }
        UpdateRowSource IDbCommand.UpdatedRowSource { get; set; }
    }
}