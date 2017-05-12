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

        protected override Expression VisitNew(NewExpression node)
        {
            var anonymousType = node.Type.Name.Contains("AnonymousType");
            if (!anonymousType)
                return node;

            var newArguments = node.Arguments.Select(ExpressionHelper.GetParameter).ToArray();
            var anonymousObject = node.Constructor.Invoke(newArguments);
            var predicateExpression = Expression.Constant(new Predicate<object>(actual => PropertiesExistsAndMatch(actual, anonymousObject)));

            return Expression.Call(null, createMatch, predicateExpression);
        }

        private static bool PropertiesExistsAndMatch(object actual, object other)
        {
            var actualParams = actual as MockDbParameterCollection ?? new MockDbParameterCollection(actual);
            var otherParams = new MockDbParameterCollection(other);

            return actualParams.Equals(otherParams);
        }
    }
}