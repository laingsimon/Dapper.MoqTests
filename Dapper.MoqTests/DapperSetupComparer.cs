namespace Dapper.MoqTests
{
    using System.Linq;
    using System.Linq.Expressions;

    internal class DapperSetupComparer
    {
        private readonly string[] dapperMethodNames;

        public DapperSetupComparer(string[] dapperMethodNames)
        {
            this.dapperMethodNames = dapperMethodNames;
        }

        public bool Matches(Expression expression)
        {
            var methodCallExpression = ExpressionHelper.GetMethodCallExpression(expression);
            if (methodCallExpression == null)
                return false;

            return dapperMethodNames.Any(name => Matches(name, methodCallExpression));
        }

        private bool Matches(string dapperMethodName, MethodCallExpression methodCallExpression)
        {
            return methodCallExpression.Method.Name == dapperMethodName;
        }
    }
}