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
            { "Query", GetMethod<object>(db => db.Query(typeof(object), "some sql", null, null, true, null, null)) },
            { "Query[T]", GetMethod<object>(db => db.Query<object>("some sql", null, null, true, null, null)) },
            { "QueryAsync", GetMethod<object>(db => db.QueryAsync(typeof(object), "some sql", null, null, null, null))},
            { "QueryAsync[T]", GetMethod<object>(db => db.QueryAsync<object>("some sql", null, null, null, null))},

            { "Query[TFirst, TSecond, TReturn]", GetMethod<object>(db => db.Query<object, object, object>("some sql", null, null, null, true, "Id", null, null)) },

            { "ExecuteImpl", GetMethod<object>(db => db.Execute("some sql", null, null, null, null)) },
            { "Execute", GetMethod<object>(db => db.Execute("some sql", null, null, null, null)) },
            { "ExecuteAsync", GetMethod<object>(db => db.ExecuteAsync("some sql", null, null, null, null))},

            { "QuerySingle", GetMethod(db => db.QuerySingle(typeof(object), "some sql", null, null, null, null))},
            { "QuerySingle[T]", GetMethod(db => db.QuerySingle<object>("some sql", null, null, null, null))},
            { "QuerySingleAsync", GetMethod<object>(db => db.QuerySingleAsync(typeof(object), "some sql", null, null, null, null))},
            { "QuerySingleAsync[T]", GetMethod<object>(db => db.QuerySingleAsync<object>("some sql", null, null, null, null))},

            { "QuerySingleOrDefault", GetMethod(db => db.QuerySingleOrDefault(typeof(object), "some sql", null, null, null, null))},
            { "QuerySingleOrDefault[T]", GetMethod(db => db.QuerySingleOrDefault<object>("some sql", null, null, null, null))},
            { "QuerySingleOrDefaultAsync", GetMethod<object>(db => db.QuerySingleOrDefaultAsync(typeof(object), "some sql", null, null, null, null))},
            { "QuerySingleOrDefaultAsync[T]", GetMethod<object>(db => db.QuerySingleOrDefaultAsync<object>("some sql", null, null, null, null))},

            { "QueryFirst", GetMethod(db => db.QueryFirst(typeof(object), "some sql", null, null, null, null))},
            { "QueryFirst[T]", GetMethod(db => db.QueryFirst<object>("some sql", null, null, null, null))},
            { "QueryFirstAsync", GetMethod(db => db.QueryFirstAsync(typeof(object), "some sql", null, null, null, null))},
            { "QueryFirstAsync[T]", GetMethod(db => db.QueryFirstAsync<object>("some sql", null, null, null, null))},

            { "QueryFirstOrDefault", GetMethod(db => db.QueryFirstOrDefault(typeof(object), "some sql", null, null, null, null))},
            { "QueryFirstOrDefault[T]", GetMethod(db => db.QueryFirstOrDefault<object>("some sql", null, null, null, null))},
            { "QueryFirstOrDefaultAsync", GetMethod(db => db.QueryFirstOrDefaultAsync(typeof(object), "some sql", null, null, null, null))},
            { "QueryFirstOrDefaultAsync[T]", GetMethod(db => db.QueryFirstOrDefaultAsync<object>("some sql", null, null, null, null))},

            { "ExecuteScalar", GetMethod(db => db.ExecuteScalar("some sql", null, null, null, null))},
            { "ExecuteScalar[T]", GetMethod(db => db.ExecuteScalar<object>("some sql", null, null, null, null))},
            { "ExecuteScalarAsync", GetMethod(db => db.ExecuteScalarAsync("some sql", null, null, null, null))},
            { "ExecuteScalarAsync[T]", GetMethod(db => db.ExecuteScalarAsync<object>("some sql", null, null, null, null))},
        };

        internal static MethodInfo GetScalar(MethodBase dapperMethod)
        {
            var methodName = dapperMethod.IsGenericMethod 
                ? $"{dapperMethod.Name}[{string.Join(", ", dapperMethod.GetGenericArguments().Select(t => t.Name))}]"
                : dapperMethod.Name;
            
            var method = Methods.ContainsKey(methodName)
                ? Methods[methodName]
                : throw new ArgumentOutOfRangeException(nameof(dapperMethod), $"Unable to find method with name `{dapperMethod.Name}`");

            return method;
        }

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
            var method = Methods.ContainsKey(dapperMethod.Name) 
                ? Methods[dapperMethod.Name]
                : throw new ArgumentOutOfRangeException(nameof(dapperMethod), $"Unable to find method with name `{dapperMethod.Name}`");
            if (dataType == null)
                return method;

            return method.GetGenericMethodDefinition().MakeGenericMethod(dataType);
        }

        public static MethodInfo GetQueryMethod(MethodBase dapperMethod, params Type[] dataTypes)
        {
            var key = dapperMethod.IsGenericMethod
                ? $"{dapperMethod.Name}[{string.Join(", ", dapperMethod.GetGenericArguments().Select(t => t.Name))}]"
                : dapperMethod.Name;

            var method = Methods[key];
            if (dataTypes.Length == 0)
                return method;

            var genericMethodDefinition = method.GetGenericMethodDefinition();
            if (genericMethodDefinition.GetGenericArguments().Length != dataTypes.Length)
                throw new InvalidOperationException($"Generic method requires {genericMethodDefinition.GetGenericArguments().Length} types, {dataTypes.Length} was provided");

            return genericMethodDefinition.MakeGenericMethod(dataTypes);
        }

        internal static bool IsSingleResultMethod(MethodInfo method)
        {
            return method.Name.StartsWith("QuerySingle");
        }
    }
}
