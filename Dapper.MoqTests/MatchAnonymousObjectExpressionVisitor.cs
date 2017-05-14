namespace Dapper.MoqTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Moq;

    internal class MatchAnonymousObjectExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo createMatch = GetCreateMatchMethod(() => Match.Create<object>(a => true));

        private static MethodInfo GetCreateMatchMethod(Expression<Func<object>> expression)
        {
            var methodCallExpression = (MethodCallExpression)expression.Body;
            return methodCallExpression.Method;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var arguments = node.Arguments.Select(ReduceToConstant).ToArray();
            var argumentLookup = arguments
                .Zip(node.Method.GetParameters(), (value, param) => new { name = param.Name, value });

            var newArguments = from argument in argumentLookup
                               select argument.name.Equals("parameters", StringComparison.OrdinalIgnoreCase)
                                    ? GetNewArgument(argument.value)
                                    : Expression.Constant(argument.value);

            return Expression.Call(node.Object, node.Method, newArguments);
        }

        private static object ReduceToConstant(Expression expression)
        {
            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private Expression GetNewArgument(object value)
        {
            var predicateExpression = Expression.Constant(new Predicate<object>(actual => PropertiesExistsAndMatch(actual, value)));
            return Expression.Call(null, createMatch, predicateExpression);
        }

        private static bool PropertiesExistsAndMatch(object actual, object other)
        {
            if (ReferenceEquals(other, MockDbConnection.Any) || ReferenceEquals(other, SqlText.Any))
                return true;

            var actualParams = actual as MockDbParameterCollection ?? new MockDbParameterCollection(actual);
            var otherParams = new MockDbParameterCollection(other);

            return actualParams.Equals(otherParams);
        }
    }
}