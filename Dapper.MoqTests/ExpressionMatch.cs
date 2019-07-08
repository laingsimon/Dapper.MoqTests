namespace Dapper.MoqTests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Moq;

    internal class ExpressionMatch : IMatch
    {
        private readonly MethodCallExpression expression;

        public ExpressionMatch(MethodCallExpression expression)
        {
            this.expression = expression;
        }

        public bool Matches(object value)
        {
            if (expression.Method.Name == nameof(It.IsAny))
                return true;

            if (expression.Method.Name == nameof(It.Is))
            {
                var methodType = expression.Method.GetGenericArguments()[0];
                var callerType = typeof(GenericMethodCaller<>).MakeGenericType(methodType);
                var caller = (GenericMethodCaller)Activator.CreateInstance(callerType);

                return caller.GetBool(expression, value);
            }

            if (expression.Method.Name == nameof(Match.Create) && expression.NodeType == ExpressionType.Call)
            {
                var predicateConstant = (ConstantExpression)expression.Arguments[0];
                var predicateValueType = predicateConstant.Value.GetType().GenericTypeArguments.Single();
                var callerType = typeof(GenericMethodCaller<>).MakeGenericType(predicateValueType);
                var caller = (GenericMethodCaller)Activator.CreateInstance(callerType);

                return caller.GetBoolFromPredicate(predicateConstant, value);
            }

            throw new NotImplementedException("How do we test this?");
        }
    }
}