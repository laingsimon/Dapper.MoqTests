namespace Dapper.MoqTests
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using Moq.Language.Flow;

    public class MockDbConnection : IDbConnection
    {
        private readonly Mock<MockDatabase> database;
        internal static readonly string Any = Guid.NewGuid().ToString();

        public MockDbConnection(MockBehavior behaviour = MockBehavior.Default)
        {
            this.database = new Mock<MockDatabase>(new object[] { behaviour });
        }

        public IDbCommand CreateCommand()
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
            RegisterExecution(expression);

            return database.As<IMockDatabase>().Setup(ModifySqlParametersArgumentInExpression(expression));
        }

        public void Verify<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            RegisterExecution(expression);

            database.As<IMockDatabase>().Verify(ModifySqlParametersArgumentInExpression(expression));
        }

        private void RegisterExecution<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            var methodCallBody = expression.Body as MethodCallExpression;
            if (methodCallBody == null)
                return;

            var arguments = methodCallBody.Arguments.Select(ExpressionHelper.GetValue).ToArray();
            var methodName = methodCallBody.Method.Name;
            var sql = (string)arguments.First(); //TODO: find the SQL parameter by name
            var parameters = arguments.Last(); //TODO: find the parameters parameter by name

            switch (methodName)
            {
                case nameof(MockDatabase.Query):
                    database.Object.ExpectReader(sql, parameters);
                    return;
                case nameof(MockDatabase.QuerySingle):
                    database.Object.ExpectScalar(sql, parameters);
                    return;
                case nameof(MockDatabase.Execute):
                    database.Object.ExpectNonQuery(sql, parameters);
                    return;
                default:
                    throw new InvalidOperationException($"Unknown method for registration: {methodName}");
            }
        }

        private Expression<Func<IMockDatabase, TReturn>> ModifySqlParametersArgumentInExpression<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            var visitor = new MatchAnonymousObjectExpressionVisitor();
            var newExpression = (Expression<Func<IMockDatabase, TReturn>>)visitor.Visit(expression);
            return newExpression;
        }
    }
}