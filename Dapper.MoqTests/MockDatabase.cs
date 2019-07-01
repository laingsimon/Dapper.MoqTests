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
        private static readonly MethodInfo queryObjectMethod = typeof(MockDatabase).GetMethod(nameof(Query)).MakeGenericMethod(typeof(object));
        private static readonly MethodInfo executeMethod = typeof(MockDatabase).GetMethod(nameof(Execute));

        private readonly MockBehavior behaviour;
        private readonly List<Expression> setups = new List<Expression>();

        protected MockDatabase(MockBehavior behaviour)
        {
            this.behaviour = behaviour;
        }

        public abstract IEnumerable<T> Query<T>(string text, object parameters = null);
        public abstract T QuerySingle<T>(string text, object parameters = null);
        public abstract int Execute(string text, object parameters = null);
        public abstract Task<IEnumerable<T>> QueryAsync<T>(string text, object parameters = null);
        public abstract Task<T> QuerySingleAsync<T>(string text, object parameters = null);
        public abstract Task<int> ExecuteAsync<T>(string text, object parameters = null);

        public void Expect(Expression setup)
        {
            this.setups.Add(setup);
        }

        public int ExecuteNonQuery(MockDbCommand command)
        {
            var parametersLookup = command.GetParameterLookup();
            var parametersArray = (from param in executeMethod.GetParameters()
                                   let commandValue = parametersLookup.ContainsKey(param.Name)
                                     ? parametersLookup[param.Name]
                                     : param.DefaultValue
                                   select commandValue).ToArray();

            return (int)executeMethod.Invoke(this, parametersArray);
        }

        public IDataReader ExecuteReader(MockDbCommand command)
        {
            var setup = FindSetup(command, nameof(Query), nameof(QuerySingle));

            var methodCall = (MethodCallExpression)setup?.Body;
            var method = methodCall?.Method ?? queryObjectMethod;
            var parametersLookup = command.GetParameterLookup();
            var parametersArray = (from param in method.GetParameters()
                                  let commandValue = parametersLookup.ContainsKey(param.Name)
                                    ? parametersLookup[param.Name]
                                    : param.DefaultValue
                                  select commandValue).ToArray();

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

        public object ExecuteScalar(MockDbCommand command)
        {
            throw new NotImplementedException("When does Dapper ever use this?");
        }

        private LambdaExpression FindSetup(MockDbCommand command, params string[] dapperExtensionMethodNames)
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
    }
}