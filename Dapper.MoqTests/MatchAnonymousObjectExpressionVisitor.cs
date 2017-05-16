namespace Dapper.MoqTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    internal class MatchAnonymousObjectExpressionVisitor : ExpressionVisitor
    {
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
                return ExpressionHelper.MakeCallToMatch<object>(argumentExpression, ParametersMatch);
            if (argumentName.Equals("text", StringComparison.OrdinalIgnoreCase))
                return ExpressionHelper.MakeCallToMatch<string>(argumentExpression, SqlCommandsMatch);

            return argumentExpression;
        }

        private static bool SqlCommandsMatch(string actual, string expected)
        {
            if (expected == SqlText.Any)
                return true;

            var actualSql = new SqlText(actual);
            var expectedSql = new SqlText(expected);

            return actualSql.Equals(expectedSql);
        }

        private static bool ParametersMatch(object actual, object expected)
        {
            if (ReferenceEquals(expected, MockDbParameterCollection.Any))
                return true;

            var actualParams = new MockDbParameterCollection(actual);
            return actualParams.Equals(expected);
        }
    }
}