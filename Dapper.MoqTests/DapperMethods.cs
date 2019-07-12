using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal static class DapperMethods
    {
        private static readonly IReadOnlyDictionary<string, MethodInfo> _dapperMethods = new Dictionary<string, MethodInfo>
        {
            { "Query[T]", GetMethod<object>(db => db.Query<object>("some sql", null, null)) },
            { "Execute", GetMethod<object>(db => db.Execute("some sql", null, null)) },
            { "QueryAsync[T]", GetMethod<object>(db => db.QueryAsync<object>("some sql", null, null))},
            { "ExecuteAsync", GetMethod<object>(db => db.ExecuteAsync("some sql", null, null))},
            { "QuerySingleAsync[T]", GetMethod<object>(db => db.QuerySingleAsync<object>("some sql", null, null))},
            { "QuerySingle[T]", GetMethod(db => db.QuerySingle<object>("some sql", null, null))}
        };

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

        public static MethodInfo GetExecuteMethod(MethodBase dapperEntrypoint)
        {
            return _dapperMethods[dapperEntrypoint.Name];
        }

        public static MethodInfo GetQueryMethod(MethodBase dapperEntrypoint)
        {
            var key = dapperEntrypoint.IsGenericMethod
                ? $"{dapperEntrypoint.Name}[{string.Join(", ", dapperEntrypoint.GetGenericArguments().Select(t => t.Name))}]"
                : dapperEntrypoint.Name;

            return _dapperMethods[key];
        }
    }
}
