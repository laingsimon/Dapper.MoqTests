namespace Dapper.MoqTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Moq;

    internal abstract class MockDatabase : IMockDatabase
    {
        private readonly MockBehavior behaviour;
        private readonly HashSet<SqlExecution> readerRegister = new HashSet<SqlExecution>();
        private readonly HashSet<SqlExecution> scalarRegister = new HashSet<SqlExecution>();
        private readonly HashSet<SqlExecution> nonQueryRegister = new HashSet<SqlExecution>();

        protected MockDatabase(MockBehavior behaviour)
        {
            this.behaviour = behaviour;
        }

        public abstract IDataReader Query<T>(string text, object parameters);
        public abstract T QuerySingle<T>(string text, object parameters);
        public abstract int Execute(string text, object parameters);

        public IDataReader ExecuteReader(string text, MockDbParameterCollection parameters)
        {
            var expected = FindExpectedExecution(text, parameters, readerRegister);
            var returnType = expected?.ReturnType ?? DeriveReturnTypeForCall("Query");

            var result = CallQuery(text, parameters, returnType);
            if (result == null) //Or is a mock....?
                return new DataTableReader(new DataTable());

            return result;
        }

        public IDataReader ExecuteQuerySingle(string text, MockDbParameterCollection parameters)
        {
            var scalarExpected = FindExpectedExecution(text, parameters, scalarRegister);
            if (scalarExpected == null)
                return ExecuteReader(text, parameters);
            var singleResult = ExecuteScalar(text, parameters);

            return singleResult.GetDataReader();
        }

        public object ExecuteScalar(string text, MockDbParameterCollection parameters)
        {
            var expected = FindExpectedExecution(text, parameters, scalarRegister);
            var returnType = expected?.ReturnType ?? DeriveReturnTypeForCall("QuerySingle");
            return CallQuerySingle(text, parameters, returnType);
        }

        private Type DeriveReturnTypeForCall(string dapperMethodToFind)
        {
            var frames = new StackTrace()
                .GetFrames()
                .Skip(3);
            var queryMethod = frames.FirstOrDefault(f => f.GetMethod().Name == dapperMethodToFind)?.GetMethod() as MethodInfo;
            if (queryMethod == null || !queryMethod.GetGenericArguments().Any())
                return typeof(object);

            var queryObjectType = queryMethod.GetGenericArguments().Single();
            if (queryObjectType.ContainsGenericParameters)
                return typeof(object);
            return queryObjectType;
        }

        public int ExecuteNonQuery(string text, MockDbParameterCollection parameters)
        {
            return Execute(text, parameters);
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

        private static void RecordSetup(string sql, object parameters, ICollection<SqlExecution> register, Type returnType = null)
        {
            var key = new SqlExecution(sql, parameters, returnType);

            if (!register.Contains(key))
                register.Add(key);
        }

        private SqlExecution FindExpectedExecution(string text, MockDbParameterCollection parameters, HashSet<SqlExecution> register)
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

        private class SqlExecution : IEquatable<SqlExecution>
        {
            public Type ReturnType { get; }
            private readonly string sql;
            private readonly object parameters;

            public SqlExecution(string sql, MockDbParameterCollection parameters)
            {
                this.sql = sql;
                this.parameters = parameters;
            }

            public SqlExecution(string sql, object parameters, Type returnType = null)
            {
                ReturnType = returnType;
                this.sql = sql;
                this.parameters = parameters;
            }

            public override int GetHashCode()
            {
                return sql.GetHashCode() ^ parameters.GetHashCode() ^ (ReturnType?.GetHashCode() ?? 0);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as SqlExecution);
            }

            public bool Equals(SqlExecution actual)
            {
                return actual != null
                    && MatchAnonymousObjectExpressionVisitor.SqlCommandsMatch(actual.sql, sql)
                    && MatchAnonymousObjectExpressionVisitor.ParametersMatch(actual.parameters, parameters)
                    && (actual.ReturnType == null || ReturnType == actual.ReturnType);
            }
        }
    }
}