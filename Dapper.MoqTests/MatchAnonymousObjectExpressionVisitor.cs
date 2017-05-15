namespace Dapper.MoqTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Moq;

    internal class MatchAnonymousObjectExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo objectCreateMatch = GetCreateMatchMethod(() => Match.Create<object>(a => true));
        private static readonly MethodInfo stringCreateMatch = GetCreateMatchMethod(() => Match.Create<string>(a => true));

        private static MethodInfo GetCreateMatchMethod(Expression<Func<object>> expression)
        {
            var methodCallExpression = (MethodCallExpression)expression.Body;
            return methodCallExpression.Method;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var arguments = node.Arguments.Select(ExpressionHelper.GetValue).ToArray();
            var argumentLookup = arguments
                .Zip(node.Method.GetParameters(), (value, param) => new {name = param.Name, value});

            var newArguments = from argument in argumentLookup
                select GetExpression(argument.name, argument.value);

            return Expression.Call(node.Object, node.Method, newArguments);
        }

        private Expression GetExpression(string argumentName, object argumentValue)
        {
            if (argumentName.Equals("parameters", StringComparison.OrdinalIgnoreCase))
                return GetMatchArgument(argumentValue, ParametersMatch);
            if (argumentName.Equals("text", StringComparison.OrdinalIgnoreCase))
                return GetMatchArgument((string)argumentValue, SqlCommandsMatch);

            return Expression.Constant(argumentValue);
        }

        private Expression GetMatchArgument(object expected, Func<object, object, bool> predicate)
        {
            var predicateExpression = Expression.Constant(new Predicate<object>(actual => predicate(actual, expected)));
            return Expression.Call(null, objectCreateMatch, predicateExpression);
        }

        private Expression GetMatchArgument(string expected, Func<string, string, bool> predicate)
        {
            var predicateExpression = Expression.Constant(new Predicate<string>(actual => predicate(actual, expected)));
            return Expression.Call(null, stringCreateMatch, predicateExpression);
        }

        private static bool SqlCommandsMatch(string actual, string expected)
        {
            if (ReferenceEquals(expected, MockDbConnection.Any))
                return true;

            var actualSql = new SqlText(actual);
            var expectedSql = new SqlText(expected);

            return actualSql.Equals(expectedSql);
        }

        private static bool ParametersMatch(object actual, object expected)
        {
            if (ReferenceEquals(expected, MockDbConnection.Any) || ReferenceEquals(expected, SqlText.Any))
                return true;

            var actualParams = actual as MockDbParameterCollection ?? new MockDbParameterCollection(actual);
            var otherParams = new MockDbParameterCollection(expected);

            return actualParams.Equals(otherParams);
        }
    }
}