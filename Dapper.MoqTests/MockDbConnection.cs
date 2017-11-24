[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Dapper.MoqTests
{
    using System;
    using System.Data;
    using System.Linq.Expressions;
    using Moq;
    using Moq.Language.Flow;

    public class MockDbConnection : IDbConnection
    {
        private readonly Mock<MockDatabase> database;

        public MockDbConnection(MockBehavior behaviour = MockBehavior.Default)
        {
            this.database = new Mock<MockDatabase>(new object[] { behaviour });
        }

        IDbCommand IDbConnection.CreateCommand()
        {
            return new MockDbCommand(database.Object);
        }

        #region non-implemented members
        void IDisposable.Dispose()
        { }

        IDbTransaction IDbConnection.BeginTransaction()
        {
            return new Mock<IDbTransaction>().Object;
        }

        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
        {
            return new Mock<IDbTransaction>().Object;
        }

        void IDbConnection.Close()
        { }

        void IDbConnection.ChangeDatabase(string databaseName)
        { }

        void IDbConnection.Open()
        { }

        string IDbConnection.ConnectionString { get; set; }

        int IDbConnection.ConnectionTimeout { get; } = 1;
        string IDbConnection.Database { get; } = "";
        ConnectionState IDbConnection.State { get; } = ConnectionState.Open;

        #endregion

        public ISetup<IMockDatabase, TReturn> Setup<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            database.Object.Expect(expression);
            return database.As<IMockDatabase>().Setup(ModifySqlParametersArgumentInExpression(expression));
        }

        public void Verify<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            database.As<IMockDatabase>().Verify(ModifySqlParametersArgumentInExpression(expression));
        }

        private Expression<Func<IMockDatabase, TReturn>> ModifySqlParametersArgumentInExpression<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            var visitor = new MatchAnonymousObjectExpressionVisitor();
            var newExpression = (Expression<Func<IMockDatabase, TReturn>>)visitor.Visit(expression);
            return newExpression;
        }
    }
}