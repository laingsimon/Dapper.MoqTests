namespace Dapper.MoqTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;

    internal static class ExpressionHelper
    {
        internal static object GetValue(Expression expression)
        {
            if (expression == null)
                return null;

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return ((ConstantExpression)expression).Value;
                case ExpressionType.New:
                    //call to an object constructor, possibly any anonymous object
                    var newExpression = (NewExpression)expression;
                    var newArguments = newExpression.Arguments.Select(GetValue).ToArray();
                    return newExpression.Constructor.Invoke(newArguments);
                case ExpressionType.MemberAccess:
                    //such as property access
                    var memberAccessExpression = (MemberExpression)expression;
                    var objectMember = Expression.Convert(memberAccessExpression, typeof(object));
                    var getterExpression = Expression.Lambda<Func<object>>(objectMember);
                    var getter = getterExpression.Compile();

                    return getter();
                case ExpressionType.Call:
                    var callExpression = (MethodCallExpression)expression;
                    if (callExpression.Method.Name == nameof(It.IsAny) && callExpression.Method.DeclaringType == typeof(It))
                        return MockDbConnection.Any;

                    var callArguments = callExpression.Arguments.Select(GetValue).ToArray();
                    return callExpression.Method.Invoke(GetValue(callExpression.Object), callArguments);
                case ExpressionType.Lambda:
                    return expression;
            }

            throw new NotSupportedException($"Expression type {expression.NodeType} is not supported");
        }
    }
}
