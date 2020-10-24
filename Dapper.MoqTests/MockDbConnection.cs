using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Moq;
using Moq.Language.Flow;

namespace Dapper.MoqTests
{
    /// <summary>
    /// A DbConnection that supports Setup and Verify of Dapper calls.
    /// </summary>
    [System.ComponentModel.DesignerCategory("Code")]
    public class MockDbConnection : DbConnection, IDbConnection
    {
        private readonly Settings _settings;
        private readonly Mock<MockDatabase> _database;

        public MockDbConnection(Settings settings = null, MockBehavior behaviour = MockBehavior.Default)
        {
            _settings = settings ?? Settings.Default;
            _database = new Mock<MockDatabase>(new object[] { behaviour });
        }

        protected override DbCommand CreateDbCommand()
        {
            return new MockDbCommand(_database.Object, _settings);
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

        public ISetup<MockDatabase, TReturn> Setup<TReturn>(Expression<Func<MockDatabase, TReturn>> expression)
        {
            return _database.Setup(ModifySqlParametersArgumentInExpression(expression));
        }

        /// <summary>Verifies that all verifiable expectations have been met.</summary>
        /// <example group="verification">
        /// 	This example sets up an expectation and marks it as verifiable. After
        /// 	the mock is used, a <c>Verify()</c> call is issued on the mock
        /// 	to ensure the method in the setup was invoked:
        /// 	<code>
        /// 		var mock = new Mock&lt;IWarehouse&gt;();
        /// 		this.Setup(x =&gt; x.HasInventory(TALISKER, 50)).Verifiable().Returns(true);
        /// 		...
        /// 		// other test code
        /// 		...
        /// 		// Will throw if the test code has didn't call HasInventory.
        /// 		this.Verify();
        /// 	</code>
        /// </example>
        /// <exception cref="T:Moq.MockException">Not all verifiable expectations were met.</exception>
        // ReSharper disable once UnusedMember.Global
        public void Verify()
        {
            _database.Verify();
        }

        /// <summary>
        /// 	Verifies that a specific invocation matching the given expression was performed on the mock. Use
        /// 	in conjunction with the default <see cref="F:Moq.MockBehavior.Loose" />.
        /// </summary>
        /// <example group="verification">
        /// 	This example assumes that the mock has been used, and later we want to verify that a given
        /// 	invocation with specific parameters was performed:
        /// 	<code>
        /// 		var mock = new Mock&lt;IWarehouse&gt;();
        /// 		// exercise mock
        /// 		//...
        /// 		// Will throw if the test code didn't call HasInventory.
        /// 		mock.Verify(warehouse =&gt; warehouse.HasInventory(TALISKER, 50));
        /// 	</code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception>
        /// <param name="expression">Expression to verify.</param>
        /// <typeparam name="TResult">Type of return value from the expression.</typeparam>
        // ReSharper disable once UnusedMember.Global
        public void Verify<TResult>(Expression<Func<MockDatabase, TResult>> expression)
        {
            _database.Verify(ModifySqlParametersArgumentInExpression(expression));
        }

        /// <summary>
        /// 	Verifies that a specific invocation matching the given
        /// 	expression was performed on the mock, specifying a failure
        /// 	error message.
        /// </summary>
        /// <example group="verification">
        /// 	This example assumes that the mock has been used,
        /// 	and later we want to verify that a given invocation
        /// 	with specific parameters was performed:
        /// 	<code>
        /// 		var mock = new Mock&lt;IWarehouse&gt;();
        /// 		// exercise mock
        /// 		//...
        /// 		// Will throw if the test code didn't call HasInventory.
        /// 		mock.Verify(warehouse =&gt; warehouse.HasInventory(TALISKER, 50), "When filling orders, inventory has to be checked");
        /// 	</code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        /// <typeparam name="TResult">Type of return value from the expression.</typeparam>
        // ReSharper disable once UnusedMember.Global
        public void Verify<TResult>(Expression<Func<MockDatabase, TResult>> expression, string failMessage)
        {
            _database.Verify(ModifySqlParametersArgumentInExpression(expression), failMessage);
        }

        /// <summary>
        /// 	Verifies that a specific invocation matching the given
        /// 	expression was performed on the mock. Use in conjunction
        /// 	with the default <see cref="F:Moq.MockBehavior.Loose" />.
        /// </summary>
        /// <exception cref="T:Moq.MockException">
        /// 	The invocation was not call the times specified by
        /// 	<paramref name="times" />.
        /// </exception>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        /// <typeparam name="TResult">Type of return value from the expression.</typeparam>
        // ReSharper disable once UnusedMember.Global
        public void Verify<TResult>(Expression<Func<MockDatabase, TResult>> expression, Times times)
        {
            _database.Verify(ModifySqlParametersArgumentInExpression(expression), times);
        }

        /// <summary>
        /// 	Verifies that a specific invocation matching the given
        /// 	expression was performed on the mock. Use in conjunction
        /// 	with the default <see cref="F:Moq.MockBehavior.Loose" />.
        /// </summary>
        /// <exception cref="T:Moq.MockException">
        /// 	The invocation was not call the times specified by
        /// 	<paramref name="times" />.
        /// </exception>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        /// <typeparam name="TResult">Type of return value from the expression.</typeparam>
        // ReSharper disable once UnusedMember.Global
        public void Verify<TResult>(Expression<Func<MockDatabase, TResult>> expression, Func<Times> times)
        {
            _database.Verify(ModifySqlParametersArgumentInExpression(expression), times);
        }

        /// <summary>
        /// 	Verifies that a specific invocation matching the given
        /// 	expression was performed on the mock, specifying a failure
        /// 	error message.
        /// </summary>
        /// <exception cref="T:Moq.MockException">
        /// 	The invocation was not call the times specified by
        /// 	<paramref name="times" />.
        /// </exception>
        /// <param name="expression">Expression to verify.</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        /// <typeparam name="TResult">Type of return value from the expression.</typeparam>
        // ReSharper disable once UnusedMember.Global
        public void Verify<TResult>(Expression<Func<MockDatabase, TResult>> expression, Times times, string failMessage)
        {
            _database.Verify(ModifySqlParametersArgumentInExpression(expression), times, failMessage);
        }

        private Expression<Func<MockDatabase, TReturn>> ModifySqlParametersArgumentInExpression<TReturn>(Expression<Func<MockDatabase, TReturn>> expression)
        {
            var visitor = new MatchAnonymousObjectExpressionVisitor(_settings);
            var newExpression = (Expression<Func<MockDatabase, TReturn>>)visitor.Visit(expression);
            return newExpression;
        }
    }
}