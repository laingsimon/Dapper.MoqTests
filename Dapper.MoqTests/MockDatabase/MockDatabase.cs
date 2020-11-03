using System;
using System.Data;
using Moq;
using System.Reflection;
using System.Data.Common;
using System.Threading;

namespace Dapper.MoqTests
{
    /// <summary>
    /// A type that represents the Dapper calls that can be intercepted and Mocked/Verified
    /// </summary>
    public abstract partial class MockDatabase
    {
        private const string NotSupported = "This method has not been proven to work with Dapper.MoqTests";
        private readonly MockBehavior _behaviour;
        private readonly DapperMethodFinder _methodFinder;

        protected MockDatabase(MockBehavior behaviour, Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _behaviour = behaviour;
            _methodFinder = new DapperMethodFinder(settings);
        }

        public abstract DbTransaction BeginTransaction(IsolationLevel il);

        internal int ExecuteNonQuery(MockDbCommand command, bool isAsync, CancellationToken cancellationToken, MethodBase dapperMethod, Type dataType)
        {
            var genericArguments = dataType == null
                ? new Type[0]
                : new[] { dataType };
            var method = _methodFinder.FindUserMethodFromMethodInCallStack(dapperMethod, genericArguments);
            var parametersLookup = command.GetParameterLookup(isAsync, cancellationToken);
            var parametersArray = method.GetValues(parametersLookup);

            return isAsync 
                ? (int)TaskHelper.GetResultOfTask(method.Invoke(this, parametersArray))
                : (int)method.Invoke(this, parametersArray);
        }

        internal object ExecuteScalar(MockDbCommand command, bool isAsync, CancellationToken cancellationToken, MethodBase dapperMethod)
        {
            var method = _methodFinder.FindUserMethodFromMethodInCallStack(dapperMethod, new Type[0]);
            var parametersLookup = command.GetParameterLookup(isAsync, cancellationToken);
            var parametersArray = method.GetValues(parametersLookup);

            var value = new ScalarValue(isAsync, method, parametersArray, this);

            return method.IsGenericMethod
                ? value
                : value.ToType(typeof(object), null);
        }

        internal IDataReader ExecuteReader(MockDbCommand command, bool isAsync, CancellationToken cancellationToken, MethodBase dapperMethod, params Type[] dataTypes)
        {
            var method = _methodFinder.FindUserMethodFromMethodInCallStack(dapperMethod, dataTypes);
            var parametersLookup = command.GetParameterLookup(isAsync, cancellationToken);
            var parametersArray = method.GetValues(parametersLookup);

            var result = method.Invoke(this, parametersArray);
            var reader = result as IDataReader;
            if (result == null)
            {
                if (IsSingleResultMethod(method))
                    return GetQuerySingleDataReader(method.GetGenericArguments()[0]);

                return GetEmptyDataReader(command);
            }

            return reader ?? result.GetDataReader();
        }

        private static bool IsSingleResultMethod(MethodInfo method)
        {
            return method.Name.StartsWith("QuerySingle");
        }

    private IDataReader GetQuerySingleDataReader(Type rowType)
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
            switch (_behaviour)
            {
                default:
                    return new DataTableReader(new DataTable());

                case MockBehavior.Strict:
                    throw new InvalidOperationException($"Unexpected call to with sql: {command.CommandText} and parameters: {command.Parameters}");
            }
        }
    }
}