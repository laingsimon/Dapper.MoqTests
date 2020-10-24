using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal static class DapperMethods
    {
        private static readonly CommandDefinition commandDefinition = new CommandDefinition();

        private static readonly IReadOnlyList<IDapperMethodInfo> Methods = new List<IDapperMethodInfo>
        {
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.Query(typeof(object), "some sql", null, null, true, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.Query<object>("some sql", null, null, true, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.QueryAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QueryAsync<object>("some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QueryAsync<object>(commandDefinition))),

            new GenericDapperMethodInfo(GetMethod<object>(db => db.Query<object, object, object>("some sql", null, null, null, true, "Id", null, null))),

            new SimpleDapperMethodInfo("ExecuteImpl", GetMethod<object>(db => db.Execute("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.Execute("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.ExecuteAsync("some sql", null, null, null, null))),

            new SimpleDapperMethodInfo(GetMethod(db => db.QuerySingle(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QuerySingle<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.QuerySingleAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QuerySingleAsync<object>("some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QuerySingleAsync<object>(commandDefinition))),

            new SimpleDapperMethodInfo(GetMethod(db => db.QuerySingleOrDefault(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QuerySingleOrDefault<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.QuerySingleOrDefaultAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QuerySingleOrDefaultAsync<object>("some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QuerySingleOrDefaultAsync<object>(commandDefinition))),

            new SimpleDapperMethodInfo(GetMethod(db => db.QueryFirst(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QueryFirst<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod(db => db.QueryFirstAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QueryFirstAsync<object>("some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QueryFirstAsync<object>(commandDefinition))),

            new SimpleDapperMethodInfo(GetMethod(db => db.QueryFirstOrDefault(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QueryFirstOrDefault<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod(db => db.QueryFirstOrDefaultAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QueryFirstOrDefaultAsync<object>("some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QueryFirstOrDefaultAsync<object>(commandDefinition))),

            new SimpleDapperMethodInfo(GetMethod(db => db.ExecuteScalar("some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.ExecuteScalar<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod(db => db.ExecuteScalarAsync("some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.ExecuteScalarAsync<object>("some sql", null, null, null, null)))
        };

        internal static MethodInfo GetScalar(MethodBase dapperMethod)
        {
            var method = Methods.SingleOrDefault(methodInfo => methodInfo.MatchesDapperMethod(dapperMethod))
                ?? throw new ArgumentOutOfRangeException(nameof(dapperMethod), $"Unable to find method with name `{dapperMethod.Name}`");

            return dapperMethod.IsGenericMethod
                ? method.GetDapperMethod(typeof(object))
                : method.GetDapperMethod();
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
            var method = Methods.SingleOrDefault(methodInfo => methodInfo.MatchesDapperMethod(dapperMethod))
                ?? throw new ArgumentOutOfRangeException(nameof(dapperMethod), $"Unable to find method with name `{dapperMethod.Name}`");
            if (dataType == null)
                return method.GetDapperMethod();

            return method.GetDapperMethod(dataType);
        }

        public static MethodInfo GetQueryMethod(MethodBase dapperMethod, params Type[] dataTypes)
        {
            var method = Methods.SingleOrDefault(methodInfo => methodInfo.MatchesDapperMethod(dapperMethod))
                ?? throw new ArgumentOutOfRangeException(nameof(dapperMethod), $"Unable to find method with name `{dapperMethod.Name}`");

            return method.GetDapperMethod(dataTypes);
        }

        internal static bool IsSingleResultMethod(MethodInfo method)
        {
            return method.Name.StartsWith("QuerySingle");
        }
    }
}
