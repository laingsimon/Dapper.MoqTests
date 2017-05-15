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

        public abstract IDataReader Query<T>(string text, object parameters);
        public abstract T QuerySingle<T>(string text, object parameters);
        public abstract int Execute(string text, object parameters);

        public IDataReader ExecuteReader(SqlText text, MockDbParameterCollection parameters)
        {
            var expected = FindExpectedExecution(text, parameters, readerRegister);

            if (expected != null)
                return CallQuery(expected.ExpectedSql, expected.ExpectedParameters, expected.ReturnType);

            var result = Query<object>(text.ToString(), parameters);
            if (result == null) //Or is a mock....?
                return new DataTableReader(new DataTable());

            return result;
        }

        public IDataReader ExecuteQuerySingle(SqlText text, MockDbParameterCollection parameters)
        {
            var singleResult = ExecuteScalar(text, parameters);
            var dataTable = new DataTable
            {
                Columns =
                {
                    { "Column0", singleResult?.GetType() ?? typeof(object) }
                },
                Rows = { singleResult }
            };

            return new DataTableReader(dataTable);
        }

        public object ExecuteScalar(SqlText text, MockDbParameterCollection parameters)
        {
            var expected = FindExpectedExecution(text, parameters, scalarRegister);
            return expected == null
                ? QuerySingle<object>(text.ToString(), parameters)
                : CallQuerySingle(expected.ExpectedSql, expected.ExpectedParameters, expected.ReturnType);
        }

        public int ExecuteNonQuery(SqlText text, MockDbParameterCollection parameters)
        {
            var expected = FindExpectedExecution(text, parameters, nonQueryRegister);
            return expected == null
                ? Execute(text.ToString(), parameters)
                : Execute(expected.GetSql(text), expected.GetParameters(parameters));
        }

        public void ExpectReader(string text, object parameters, Type rowType)
        {
            RecordSetup(text, parameters, readerRegister, rowType);
        }

        public void ExpectScalar(string text, object parameters, Type objectType)
        {
            RecordSetup(text, parameters, scalarRegister, objectType);
        }

        public void ExpectNonQuery(string text, object parameters)
        {
            RecordSetup(text, parameters, nonQueryRegister);
        }

        private IDataReader CallQuery(string text, object parameters, Type returnType)
        {
            var genericDatabase = GetGenericDatabase(returnType);
            return genericDatabase.Query(text, parameters);
        }

        private object CallQuerySingle(string text, object parameters, Type returnType)
        {
            var genericDatabase = GetGenericDatabase(returnType);
            return genericDatabase.QuerySingle(text, parameters);
        }

        private IGenericMockDatabase GetGenericDatabase(Type returnType)
        {
            var genericDatabaseType = typeof(GenericMockDatabase<>).MakeGenericType(returnType);
            return (IGenericMockDatabase)Activator.CreateInstance(genericDatabaseType, this);
        }

        private static void RecordSetup(string sql, object parameters, ICollection<ExpectedExecution> register, Type returnType = null)
        {
            var key = new ExpectedExecution(sql, parameters, returnType);

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
            public Type ReturnType { get; }

            public ExpectedExecution(string sql, object parameters, Type returnType)
                : base(SqlText.Create(sql), MockDbParameterCollection.Create(parameters))
            {
                ExpectedSql = sql;
                ExpectedParameters = parameters;
                ReturnType = returnType;
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