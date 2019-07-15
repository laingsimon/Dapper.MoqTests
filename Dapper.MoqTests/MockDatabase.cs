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
    using System.Data.Common;

    internal abstract class MockDatabase : IMockDatabase
    {
        private readonly MockBehavior behaviour;
        private readonly List<Expression> setups = new List<Expression>();

        protected MockDatabase(MockBehavior behaviour)
        {
            this.behaviour = behaviour;
        }

        public abstract IEnumerable<T> Query<T>(string text, object parameters = null, IDbTransaction transaction = null);
        public abstract T QuerySingle<T>(string text, object parameters = null, IDbTransaction transaction = null);
        public abstract int Execute(string text, object parameters = null, IDbTransaction transaction = null);
        public abstract Task<IEnumerable<T>> QueryAsync<T>(string text, object parameters = null, IDbTransaction transaction = null);
        public abstract Task<T> QuerySingleAsync<T>(string text, object parameters = null, IDbTransaction transaction = null);
        public abstract Task<int> ExecuteAsync(string text, object parameters = null, IDbTransaction transaction = null);
        public abstract DbTransaction BeginTransaction(IsolationLevel il);

        public void Expect(Expression setup)
        {
            this.setups.Add(setup);
        }

        public int ExecuteNonQuery(MockDbCommand command, bool isAsync, MethodBase dapperEntrypoint, Type dataType)
        {
            var method = DapperMethods.GetExecuteMethod(dapperEntrypoint, dataType);
            var parametersLookup = command.GetParameterLookup();
            var parametersArray = method.GetValues(parametersLookup);

            return isAsync 
                ? ((Task<int>)method.Invoke(this, parametersArray)).Result
                : (int)method.Invoke(this, parametersArray);
        }

        public IDataReader ExecuteReader(MockDbCommand command, MethodBase dapperEntrypoint, Type dataType)
        {
            var sourceMethod = DapperMethods.GetQueryMethod(dapperEntrypoint, dataType);
            var setup = FindSetup(command, sourceMethod);

            var methodCall = (MethodCallExpression)setup?.Body;
            var method = methodCall?.Method ?? sourceMethod;
            var parametersLookup = command.GetParameterLookup();
            var parametersArray = method.GetValues(parametersLookup);

            var result = method.Invoke(this, parametersArray);
            var reader = result as IDataReader;
            if (result == null)
            {
                if (DapperMethods.IsSingleResultMethod(method))
                    return GetQuerySingleDataReader(command, method.GetGenericArguments()[0]);

                return GetEmptyDataReader(command);
            }

            return reader ?? result.GetDataReader();
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

        public object ExecuteScalar(MockDbCommand command, MethodBase dapperEntrypoint, Type dataType)
        {
            throw new NotImplementedException("When does Dapper ever use this?");
        }

        private LambdaExpression FindSetup(MockDbCommand command, MethodInfo methodToFind)
        {
            var comparer = new DapperSetupComparer(methodToFind);
            var expression = this.setups.SingleOrDefault(comparer.Matches) as LambdaExpression;
            if (expression == null)
                return null;

            var methodCallComparer = DapperMethodCallComparer.GetComparerForExpression(expression);
            if (methodCallComparer.CommandMatchesExpression(command))
                return expression;

            return null;
        }
    }
}