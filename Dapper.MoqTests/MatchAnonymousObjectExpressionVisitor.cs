using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal class MatchAnonymousObjectExpressionVisitor : ExpressionVisitor
    {
        private readonly Settings _settings;

        public MatchAnonymousObjectExpressionVisitor(Settings settings)
        {
            _settings = settings;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var argumentLookup = node.Arguments
                .Zip(node.Method.GetParameters(), (argExpression, param) => new { param, argExpression });

            var newArguments = from argument in argumentLookup
                select GetExpression(argument.param, argument.argExpression);

            return Expression.Call(node.Object, node.Method, newArguments);
        }

        private Expression GetExpression(ParameterInfo parameter, Expression argumentExpression)
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

        private bool SqlCommandsMatch(string actual, string expected)
        {
            return _settings.SqlTextComparer.Equals(actual, expected);
        }

        private bool ParametersMatch(object actual, object expected)
        {
            if (actual == null)
                return expected == null;

            return _settings.SqlParametersComparer.Equals(actual, expected);
        }
    }
}