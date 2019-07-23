using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal static class DapperMethods
    {
        private static readonly IReadOnlyDictionary<string, MethodInfo> Methods = new Dictionary<string, MethodInfo>
        {
            { "Query[T]", GetMethod<object>(db => db.Query<object>("some sql", null, null, true, null, null)) },
            { "Execute", GetMethod<object>(db => db.Execute("some sql", null, null, null, null)) },
            { "QueryAsync[T]", GetMethod<object>(db => db.QueryAsync<object>("some sql", null, null, null, null))},
            { "ExecuteAsync", GetMethod<object>(db => db.ExecuteAsync("some sql", null, null, null, null))},
            { "QuerySingleAsync[T]", GetMethod<object>(db => db.QuerySingleAsync<object>("some sql", null, null, null, null))},
            { "QuerySingle[T]", GetMethod(db => db.QuerySingle<object>("some sql", null, null, null, null))}
        };

        private static MethodInfo GetMethod<TOut>(Expression<Func<MockDatabase, TOut>> expression)
        {
            if (expression.Body is UnaryExpression unaryExpression)
            {
                var unaryExpressionOperand = (MethodCallExpression)unaryExpression.Operand;
                return unaryExpressionOperand.Method;
            }

            var methodCallExpression = (MethodCallExpression)expression.Body;
            return methodCallExpression.Method;
        }

        public static MethodInfo GetExecuteMethod(MethodBase dapperMethod, Type dataType)
        {
            var method = Methods[dapperMethod.Name];
            if (dataType == null)
                return method;

            return method.GetGenericMethodDefinition().MakeGenericMethod(dataType);
        }

        public static MethodInfo GetQueryMethod(MethodBase dapperMethod, Type dataType)
        {
            var key = dapperMethod.IsGenericMethod
                ? $"{dapperMethod.Name}[{string.Join(", ", dapperMethod.GetGenericArguments().Select(t => t.Name))}]"
                : dapperMethod.Name;

            var method = Methods[key];
            if (dataType == null)
                return method;

            return method.GetGenericMethodDefinition().MakeGenericMethod(dataType);
        }

        internal static bool IsSingleResultMethod(MethodInfo method)
        {
            return method.Name.StartsWith("QuerySingle");
        }
    }
}
