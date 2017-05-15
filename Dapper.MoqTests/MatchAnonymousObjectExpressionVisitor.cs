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
            var argumentLookup = node.Arguments
                .Zip(node.Method.GetParameters(), (argExpression, param) => new {name = param.Name, argExpression });

            var newArguments = from argument in argumentLookup
                select GetExpression(argument.name, argument.argExpression);

            return Expression.Call(node.Object, node.Method, newArguments);
        }

        private Expression GetExpression(string argumentName, Expression argumentExpression)
        {
            if (argumentName.Equals("parameters", StringComparison.OrdinalIgnoreCase))
                return GetMatchArgument(ExpressionHelper.GetValue(argumentExpression), ParametersMatch);
            if (argumentName.Equals("text", StringComparison.OrdinalIgnoreCase))
                return GetMatchArgument((string)ExpressionHelper.GetValue(argumentExpression), SqlCommandsMatch);

            return argumentExpression;
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

        public static bool SqlCommandsMatch(string actual, string expected)
        {
            if (expected == SqlText.Any)
                return true;

            var actualSql = new SqlText(actual);
            var expectedSql = new SqlText(expected);

            return actualSql.Equals(expectedSql);
        }

        public static bool ParametersMatch(object actual, object expected)
        {
            if (ReferenceEquals(expected, MockDbParameterCollection.Any))
                return true;

            var actualParams = (MockDbParameterCollection)actual;
            return actualParams.Equals(expected);
        }
    }
}