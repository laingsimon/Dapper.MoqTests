namespace Dapper.MoqTests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;
    using System.Reflection;
    using System.Threading.Tasks;

    internal abstract class MockDatabase : IMockDatabase
    {
        private static readonly MethodInfo queryObjectMethod = GetMethod<object>(db => db.Query<object>("some sql", null, null));
        private static readonly MethodInfo executeMethod = GetMethod<object>(db => db.Execute("some sql", null, null));
        private static readonly MethodInfo queryObjectAsyncMethod = GetMethod<object>(db => db.QueryAsync<object>("some sql", null, null));
        private static readonly MethodInfo executeAsyncMethod = GetMethod<object>(db => db.ExecuteAsync("some sql", null, null));

        private readonly MockBehavior behaviour;
        private readonly List<Expression> setups = new List<Expression>();

        protected MockDatabase(MockBehavior behaviour)
        {
            this.behaviour = behaviour;
        }

        public IEnumerable<T> Query<T>(string text) => Query<T>(text, null);
        public T QuerySingle<T>(string text) => QuerySingle<T>(text, null);
        public int Execute(string text) => Execute(text, null);
        public Task<IEnumerable<T>> QueryAsync<T>(string text) => QueryAsync<T>(text, null);
        public Task<T> QuerySingleAsync<T>(string text) => QuerySingleAsync<T>(text, null);
        public Task<int> ExecuteAsync(string text) => ExecuteAsync(text, null);

        public abstract IEnumerable<T> Query<T>(string text, object parameters);
        public abstract T QuerySingle<T>(string text, object parameters);
        public abstract int Execute(string text, object parameters);
        public abstract Task<IEnumerable<T>> QueryAsync<T>(string text, object parameters);
        public abstract Task<T> QuerySingleAsync<T>(string text, object parameters);
        public abstract Task<int> ExecuteAsync(string text, object parameters);

        public abstract IEnumerable<T> Query<T>(string text, object parameters, IDbTransaction transaction);
        public abstract T QuerySingle<T>(string text, object parameters, IDbTransaction transaction);
        public abstract int Execute(string text, object parameters, IDbTransaction transaction);
        public abstract Task<IEnumerable<T>> QueryAsync<T>(string text, object parameters, IDbTransaction transaction);
        public abstract Task<T> QuerySingleAsync<T>(string text, object parameters, IDbTransaction transaction);
        public abstract Task<int> ExecuteAsync(string text, object parameters, IDbTransaction transaction);

        public void Expect(Expression setup)
        {
            this.setups.Add(setup);
        }

        public int ExecuteNonQuery(MockDbCommand command, bool isAsync)
        {
            var method = isAsync ? executeAsyncMethod : executeMethod;
            var parametersLookup = command.GetParameterLookup();
            var parametersArray = method.GetValues(parametersLookup);

            return isAsync 
                ? ((Task<int>)method.Invoke(this, parametersArray)).Result
                : (int)method.Invoke(this, parametersArray);
        }

        public IDataReader ExecuteReader(MockDbCommand command, bool isAsync, bool? singleRow)
        {
            var setup = FindSetup(command, GetDapperMethodNames(isAsync, singleRow));

            var sourceMethod = isAsync ? queryObjectAsyncMethod : queryObjectMethod;
            var methodCall = (MethodCallExpression)setup?.Body;
            var method = methodCall?.Method ?? sourceMethod;
            var parametersLookup = command.GetParameterLookup();
            var parametersArray = method.GetValues(parametersLookup);

            var result = method.Invoke(this, parametersArray);
            var reader = result as IDataReader;
            if (result == null)
            {
                if (method.Name == nameof(QuerySingle))
                    return GetQuerySingleDataReader(command, method.GetGenericArguments()[0]);

                return GetEmptyDataReader(command);
            }

            return reader ?? result.GetDataReader();
        }

        private string[] GetDapperMethodNames(bool isAsync, bool? singleRow)
        {
            if (singleRow == true)
            {
                return new[] {isAsync ? nameof(QuerySingleAsync) : nameof(QuerySingle)};
            }

            if (singleRow == false)
            {
                return new[] { isAsync ? nameof(QueryAsync) : nameof(Query) };
            }

            return isAsync
                ? new[] {nameof(QueryAsync), nameof(QuerySingleAsync)}
                : new[] {nameof(Query), nameof(QuerySingle)};
        }

        private IDataReader GetQuerySingleDataReader(IDbCommand command, Type rowType)
        {
            if (Nullable.GetUnderlyingType(rowType) != null)
                rowType = typeof(object); //because DataTable doesn't support Nullable<T> as a column-type

            var dataTable = new DataTable
            {
                Columns =
                {
                    { "Column0", rowType }
                }
            };

            dataTable.Rows.Add((object)null);

            return new DataTableReader(dataTable);
        }

        private DataTableReader GetEmptyDataReader(IDbCommand command)
        {
            switch (behaviour)
            {
                default:
                    return new DataTableReader(new DataTable());

                case MockBehavior.Strict:
                    throw new InvalidOperationException($"Unexpected call to with sql: {command.CommandText} and parameters: {command.Parameters}");
            }
        }

        public object ExecuteScalar(MockDbCommand command, bool isAsync)
        {
            throw new NotImplementedException("When does Dapper ever use this?");
        }

        private LambdaExpression FindSetup(MockDbCommand command, string[] dapperExtensionMethodNames)
        {
            var comparer = new DapperSetupComparer(dapperExtensionMethodNames);
            var expression = this.setups.SingleOrDefault(comparer.Matches) as LambdaExpression;
            if (expression == null)
                return null;

            var methodCallComparer = DapperMethodCallComparer.GetComparerForExpression(expression);
            if (methodCallComparer.CommandMatchesExpression(command))
                return expression;

            return null;
        }

        private static MethodInfo GetMethod<TOut>(Expression<Func<MockDatabase, TOut>> expression)
        {
            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null)
            {
                var unaryExpressionOperand = (MethodCallExpression)unaryExpression.Operand;
                return unaryExpressionOperand.Method;
            }

            var methodCallExpression = (MethodCallExpression)expression.Body;
            return methodCallExpression.Method;
        }
    }
}