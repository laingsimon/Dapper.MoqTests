using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Moq;
using Moq.Language.Flow;

namespace Dapper.MoqTests
{
    public class MockDbConnection : DbConnection, IDbConnection
    {
        private readonly Mock<MockDatabase> _database;

        public MockDbConnection(MockBehavior behaviour = MockBehavior.Default)
        {
            _database = new Mock<MockDatabase>(new object[] { behaviour });
        }

        protected override DbCommand CreateDbCommand()
        {
            return new MockDbCommand(_database.Object);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _database.Object.BeginTransaction(isolationLevel) ?? new MockDbTransaction(this, isolationLevel);
        }

        #region non-implemented members
        void IDisposable.Dispose()
        { }

        public override void Close()
        { }

        public override void ChangeDatabase(string databaseName)
        { }

        public override void Open()
        { }

        public override string ConnectionString { get; set; }
        public override int ConnectionTimeout { get; } = 1;
        public override string Database { get; } = "";
        public override ConnectionState State { get; } = ConnectionState.Open;
        public override string DataSource { get; } = "";
        public override string ServerVersion { get; } = "";

        #endregion

        public ISetup<IMockDatabase, TReturn> Setup<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            _database.Object.Expect(expression);
            return _database.As<IMockDatabase>().Setup(ModifySqlParametersArgumentInExpression(expression));
        }

        public void Verify()
        {
            _database.As<IMockDatabase>().Verify();
        }

        public void Verify<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            _database.As<IMockDatabase>().Verify(ModifySqlParametersArgumentInExpression(expression));
        }

        public void Verify<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression, string failMessage)
        {
            _database.As<IMockDatabase>().Verify(ModifySqlParametersArgumentInExpression(expression), failMessage);
        }

        public void Verify<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression, Times times)
        {
            _database.As<IMockDatabase>().Verify(ModifySqlParametersArgumentInExpression(expression), times);
        }

        public void Verify<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression, Func<Times> times)
        {
            _database.As<IMockDatabase>().Verify(ModifySqlParametersArgumentInExpression(expression), times);
        }

        public void Verify<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression, Times times, string failMessage)
        {
            _database.As<IMockDatabase>().Verify(ModifySqlParametersArgumentInExpression(expression), times, failMessage);
        }

        private static Expression<Func<IMockDatabase, TReturn>> ModifySqlParametersArgumentInExpression<TReturn>(Expression<Func<IMockDatabase, TReturn>> expression)
        {
            var visitor = new MatchAnonymousObjectExpressionVisitor();
            var newExpression = (Expression<Func<IMockDatabase, TReturn>>)visitor.Visit(expression);
            return newExpression;
        }
    }
}
