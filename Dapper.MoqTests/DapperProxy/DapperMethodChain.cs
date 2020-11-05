using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.MoqTests
{
    public class DapperMethodChain
    {
        public MethodInfo DapperMethodInCallStack { get; }
        public MethodInfo[] PotentialDapperMethodCalls { get; }

        private DapperMethodChain(MethodInfo dapperMethodInCallStack, params MethodInfo[] potentialDapperMethodCalls)
        {
            DapperMethodInCallStack = dapperMethodInCallStack;
            PotentialDapperMethodCalls = potentialDapperMethodCalls;
        }

        public static DapperMethodChain ForCallInStack(MethodInfo dapperMethodInCallStack)
        {
            return new DapperMethodChain(dapperMethodInCallStack);
        }

        internal static DapperMethodChain ForCallInStack<T>(Expression<Func<MockDatabase, T>> dapperMethodInCallStack)
        {
            return ForCallInStack(GetMethod(dapperMethodInCallStack));
        }

        public DapperMethodChain WithUserCall(MethodInfo potentialDapperMethodCall)
        {
            return new DapperMethodChain(DapperMethodInCallStack, PotentialDapperMethodCalls.Concat(new[] { potentialDapperMethodCall }).ToArray());
        }

        public DapperMethodChain WithUserCall<T>(Expression<Func<MockDatabase, T>> expression)
        {
            return WithUserCall(GetMethod(expression));
        }

        private static MethodInfo GetMethod<T>(Expression<Func<MockDatabase, T>> expression)
        {
            if (expression.Body is UnaryExpression unaryExpression)
            {
                var unaryExpressionOperand = (MethodCallExpression)unaryExpression.Operand;
                return unaryExpressionOperand.Method;
            }

            var methodCallExpression = (MethodCallExpression)expression.Body;
            return methodCallExpression.Method;
        }

        public MethodInfo FindMethod(MethodBase dapperMethod, Type[] genericMethodArguments)
        {
            if (PotentialDapperMethodCalls.Length == 0)
            {
                throw new InvalidOperationException($"No potential dapper methods defined in this chain: {DapperMethodInCallStack}");
            }

            if (PotentialDapperMethodCalls.Length == 1)
            {
                return PotentialDapperMethodCalls.Single();
            }

            throw new NotImplementedException($"Need to find a way to detect which of the following methods should be returned:\r\n{string.Join("\r\n", PotentialDapperMethodCalls.Select(m => m.ToString()))}\r\nDapper method: {dapperMethod}");
        }

        public override string ToString()
        {
            return $"DapperMethodChain: {DapperMethodInCallStack} --> {PotentialDapperMethodCalls.Length} potential method call/s";
        }
    }
}
