using System.Reflection;

namespace Dapper.MoqTests
{
    using System.Linq.Expressions;

    internal class DapperSetupComparer
    {
        private readonly MethodInfo _methodToFind;

        public DapperSetupComparer(MethodInfo methodToFind)
        {
            _methodToFind = methodToFind;
        }

        public bool Matches(Expression expression)
        {
            var methodCallExpression = ExpressionHelper.GetMethodCallExpression(expression);
            if (methodCallExpression == null)
                return false;

            return _methodToFind.Name == methodCallExpression.Method.Name
                   && _methodToFind.IsGenericMethod == methodCallExpression.Method.IsGenericMethod
                   && _methodToFind.GetGenericArguments().Length ==
                   methodCallExpression.Method.GetGenericArguments().Length;
        }
    }
}