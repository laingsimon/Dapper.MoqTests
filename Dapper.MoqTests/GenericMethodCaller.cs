using System;
using System.Linq.Expressions;

namespace Dapper.MoqTests
{
    internal abstract class GenericMethodCaller
    {
        public abstract bool GetBool(MethodCallExpression expression, object input);
        public abstract bool GetBoolFromPredicate(ConstantExpression predicateConstant, object value);
    }

    internal class GenericMethodCaller<T> : GenericMethodCaller
    {
        public override bool GetBool(MethodCallExpression expression, object input)
        {
            var unaryExpression = (UnaryExpression)expression.Arguments[0];
            var predicateExpression = (Expression<Func<T, bool>>)unaryExpression.Operand;
            var predicate = predicateExpression.Compile();

            return predicate((T)input);
        }

        public override bool GetBoolFromPredicate(ConstantExpression predicateConstant, object value)
        {
            var predicate = (Predicate<T>)predicateConstant.Value;
            return predicate.Invoke((T)value);
        }
    }
}
