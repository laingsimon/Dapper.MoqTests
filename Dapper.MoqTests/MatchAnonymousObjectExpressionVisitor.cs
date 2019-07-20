using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal class MatchAnonymousObjectExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var argumentLookup = node.Arguments
                .Zip(node.Method.GetParameters(), (argExpression, param) => new { param, argExpression });

            var newArguments = from argument in argumentLookup
                select GetExpression(argument.param, argument.argExpression);

            return Expression.Call(node.Object, node.Method, newArguments);
        }

        private static Expression GetExpression(ParameterInfo parameter, Expression argumentExpression)
        {
            var propertyType = parameter.GetCustomAttribute<ParameterTypeAttribute>()?.Type;

            switch (propertyType)
            {
                case ParameterType.SqlText:
                    return ExpressionHelper.MakeCallToMatch<string>(argumentExpression, SqlCommandsMatch);

                case ParameterType.SqlParameters:
                    return ExpressionHelper.MakeCallToMatch<object>(argumentExpression, ParametersMatch);
            }

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

            if (actual == null)
                return expected == null;

            var actualParams = new MockDbParameterCollection(actual);
            return actualParams.Equals(expected);
        }
    }
}