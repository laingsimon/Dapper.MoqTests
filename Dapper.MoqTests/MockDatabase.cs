namespace Dapper.MoqTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Moq;

    internal abstract class MockDatabase : IMockDatabase
    {
        private readonly MockBehavior behaviour;
        private readonly HashSet<ExpectedExecution> readerRegister = new HashSet<ExpectedExecution>();
        private readonly HashSet<ExpectedExecution> scalarRegister = new HashSet<ExpectedExecution>();
        private readonly HashSet<ExpectedExecution> nonQueryRegister = new HashSet<ExpectedExecution>();

        protected MockDatabase(MockBehavior behaviour)
        {
            this.behaviour = behaviour;
        }

        public abstract IDataReader Query(string text, object parameters);
        public abstract object QuerySingle(string text, object parameters);
        public abstract int Execute(string text, object parameters);

        public IDataReader ExecuteReader(SqlText text, MockDbParameterCollection parameters)
        {
            var expected = FindExpectedExecution(text, parameters, readerRegister);

            if (expected != null)
                return Query(expected.ExpectedSql, expected.ExpectedParameters);

            var result = Query(text.ToString(), parameters);
            if (result == null) //Or is a mock....?
                return new DataTableReader(new DataTable());

            return result;
        }

        public object ExecuteScalar(SqlText text, MockDbParameterCollection parameters)
        {
            var expected = FindExpectedExecution(text, parameters, scalarRegister);
            return expected == null
                ? QuerySingle(text.ToString(), parameters)
                : QuerySingle(expected.ExpectedSql, expected.ExpectedParameters);
        }

        public int ExecuteNonQuery(SqlText text, MockDbParameterCollection parameters)
        {
            var expected = FindExpectedExecution(text, parameters, nonQueryRegister);
            return expected == null
                ? Execute(text.ToString(), parameters)
                : Execute(expected.GetSql(text), expected.GetParameters(parameters));
        }

        public void ExpectReader(string text, object parameters)
        {
            RecordSetup(text, parameters, readerRegister);
        }

        public void ExpectScalar(string text, object parameters)
        {
            RecordSetup(text, parameters, scalarRegister);
        }

        public void ExpectNonQuery(string text, object parameters)
        {
            RecordSetup(text, parameters, nonQueryRegister);
        }

        private static void RecordSetup(string sql, object parameters, ICollection<ExpectedExecution> register)
        {
            var key = new ExpectedExecution(sql, parameters);

            if (!register.Contains(key))
                register.Add(key);
        }

        private ExpectedExecution FindExpectedExecution(SqlText text, MockDbParameterCollection parameters, HashSet<ExpectedExecution> register)
        {
            var actual = new SqlExecution(text, parameters);
            var matching = register.Where(ee => ee.Equals(actual)).ToArray();

            if (!matching.Any())
            {
                if (behaviour == MockBehavior.Strict)
                    throw new InvalidOperationException($"Unexpected call with arguments ({text}, {parameters})");

                return null;
            }

            if (matching.Length == 1)
                return matching.Single();

            throw new InvalidOperationException("Multiple executions match the given statement and parameters, have you setup the method twice?");
        }

        private class ExpectedExecution : SqlExecution, IEquatable<ExpectedExecution>
        {
            public string ExpectedSql { get; }
            public object ExpectedParameters { get; }

            public ExpectedExecution(string sql, object parameters)
                : base(SqlText.Create(sql), MockDbParameterCollection.Create(parameters))
            {
                ExpectedSql = sql;
                ExpectedParameters = parameters;
            }

            public string GetSql(SqlText actual)
            {
                return ReferenceEquals(Sql, SqlText.Any)
                    ? actual.ToString()
                    : ExpectedSql;
            }

            public object GetParameters(MockDbParameterCollection actual)
            {
                return ReferenceEquals(Parameters, MockDbParameterCollection.Any)
                    ? actual
                    : Parameters;
            }

            public override int GetHashCode()
            {
                return ExpectedSql.GetHashCode() ^ ExpectedParameters.GetHashCode();
            }

            public bool Equals(ExpectedExecution other)
            {
                return other != null
                       && ExpectedSql.Equals(other.ExpectedSql)
                       && ExpectedParameters.Equals(other.ExpectedParameters);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ExpectedExecution);
            }
        }

        private class SqlExecution : IEquatable<SqlExecution>
        {
            protected readonly SqlText Sql;
            protected readonly MockDbParameterCollection Parameters;

            public SqlExecution(SqlText sql, MockDbParameterCollection parameters)
            {
                this.Sql = sql;
                this.Parameters = parameters;
            }

            public bool Equals(SqlExecution actual)
            {
                return actual != null
                       && (ReferenceEquals(Sql, SqlText.Any) || Sql.Equals(actual.Sql))
                       && (ReferenceEquals(Parameters, MockDbParameterCollection.Any) || Parameters.Equals(actual.Parameters));
            }
        }
    }
}