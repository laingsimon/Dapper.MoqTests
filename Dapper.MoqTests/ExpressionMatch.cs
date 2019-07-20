using System;
using System.Linq;
using System.Linq.Expressions;
using Moq;

namespace Dapper.MoqTests
{
    internal class ExpressionMatch : IMatch
    {
        private readonly MethodCallExpression _expression;

        public ExpressionMatch(MethodCallExpression expression)
        {
            _expression = expression;
        }

        public bool Matches(object value)
        {
            if (_expression.Method.Name == nameof(It.IsAny))
                return true;

            if (_expression.Method.Name == nameof(It.Is))
            {
                var methodType = _expression.Method.GetGenericArguments()[0];
                var callerType = typeof(GenericMethodCaller<>).MakeGenericType(methodType);
                var caller = (GenericMethodCaller)Activator.CreateInstance(callerType);

                return caller.GetBool(_expression, value);
            }

            if (_expression.Method.Name == nameof(Match.Create) && _expression.NodeType == ExpressionType.Call)
            {
                var predicateConstant = (ConstantExpression)_expression.Arguments[0];
                var predicateValueType = predicateConstant.Value.GetType().GenericTypeArguments.Single();
                var callerType = typeof(GenericMethodCaller<>).MakeGenericType(predicateValueType);
                var caller = (GenericMethodCaller)Activator.CreateInstance(callerType);

                return caller.GetBoolFromPredicate(predicateConstant, value);
            }

            throw new NotImplementedException("How do we test this?");
        }
    }
}