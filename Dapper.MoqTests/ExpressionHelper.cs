namespace Dapper.MoqTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Moq;

    internal static class ExpressionHelper
    {
        public static T GetValueFromExpression<T>(Expression expression)
        {
            if (expression == null)
                return default(T);

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return (T)((ConstantExpression)expression).Value;
                case ExpressionType.New:
                    //call to an object constructor, possibly any anonymous object
                    var newExpression = (NewExpression)expression;
                    var newArguments = newExpression.Arguments.Select(GetValueFromExpression<object>).ToArray();
                    return (T)newExpression.Constructor.Invoke(newArguments);
                case ExpressionType.MemberAccess:
                    //such as property access
                    var memberAccessExpression = (MemberExpression)expression;
                    var objectMember = Expression.Convert(memberAccessExpression, typeof(object));
                    var getterExpression = Expression.Lambda<Func<object>>(objectMember);
                    var getter = getterExpression.Compile();

                    return (T)getter();
                case ExpressionType.Call:
                    var callExpression = (MethodCallExpression)expression;
                    var callArguments = callExpression.Arguments.Select(GetValueFromExpression<object>).ToArray();
                    return (T)callExpression.Method.Invoke(GetValueFromExpression<object>(callExpression.Object), callArguments);
            }

            throw new NotSupportedException($"Expression type {expression.NodeType} is not supported");
        }

        public static Expression MakeCallToMatch<T>(Expression expression, Func<T, T, bool> predicate)
        {
            var callExpression = expression as MethodCallExpression;
            if (callExpression != null && callExpression.Method.DeclaringType == typeof(It))
                return expression; //already a call to a match expression

            var expected = GetValueFromExpression<T>(expression);
            var createMatch = GetCreateMatchMethod<T>();
            var predicateExpression = Expression.Constant(new Predicate<T>(actual => predicate(actual, expected)));
            return Expression.Call(null, createMatch, predicateExpression);
        }

        private static MethodInfo GetCreateMatchMethod<T>()
        {
            return GetCreateMatchMethod(() => Match.Create<T>(_ => true));
        }

        private static MethodInfo GetCreateMatchMethod<T>(Expression<Func<T>> expression)
        {
            var methodCallExpression = (MethodCallExpression)expression.Body;
            return methodCallExpression.Method;
        }

        public static MethodCallExpression GetMethodCallExpression(Expression expression)
        {
            var methodCallExpression = expression as MethodCallExpression;

            if (methodCallExpression != null)
                return methodCallExpression;

            var lambdaExpression = expression as LambdaExpression;
            if (lambdaExpression == null)
                return null;

            return GetMethodCallExpression(lambdaExpression.Body);
        }
    }
}
