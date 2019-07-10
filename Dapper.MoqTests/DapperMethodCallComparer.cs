namespace Dapper.MoqTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Moq;

    internal class DapperMethodCallComparer
    {
        private readonly MethodCallExpression methodCallExpression;

        public static DapperMethodCallComparer GetComparerForExpression(Expression expression)
        {
            var methodCallExpression = ExpressionHelper.GetMethodCallExpression(expression);
            if (methodCallExpression == null)
                return null;

            return new DapperMethodCallComparer(methodCallExpression);
        }

        private DapperMethodCallComparer(MethodCallExpression methodCallExpression)
        {
            this.methodCallExpression = methodCallExpression;
        }

        public bool CommandMatchesExpression(MockDbCommand command)
        {
            var visitor = new MatchAnonymousObjectExpressionVisitor();
            var comparisonVisitor = (MethodCallExpression)visitor.Visit(methodCallExpression);

            var call = comparisonVisitor.Method.GetParameters()
                .Zip(comparisonVisitor.Arguments, (methodArg, callArg) => new { methodArg, callArg })
                .ToDictionary(a => a.methodArg, a => a.callArg);

            var expectedParameters = command.GetParameterLookup();
            var parameterValues = from arg in call
                let match = ResolveToMatch(arg.Value as MethodCallExpression)
                let paramValue = expectedParameters.GetValue(arg.Key)
                select new { arg, matches = match != null && match.Matches(paramValue) };

            return parameterValues.All(a => a.matches);
        }

        private static IMatch ResolveToMatch(MethodCallExpression expression)
        {
            if (expression == null)
                return null;

            var method = expression.Method;

            if (method.DeclaringType == typeof(It) || method.DeclaringType == typeof(Match))
                return new ExpressionMatch(expression);

            var matchType = typeof(Match<>).MakeGenericType(method.GetGenericArguments()[0]);
            var newArguments = expression.Arguments.Select(ExpressionHelper.GetValueFromExpression<object>).ToArray();
            var constructor = matchType
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .SingleOrDefault(c => c.GetParameters().Length == newArguments.Length);

            if (constructor == null)
                throw new InvalidOperationException($"Could not find appropriate constructor on type: {nameof(Match)}");
            return new MatchProxy((Match)constructor.Invoke(newArguments));
        }
    }
}
