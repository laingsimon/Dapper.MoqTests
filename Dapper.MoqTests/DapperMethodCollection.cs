using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.MoqTests
{
    public abstract class DapperMethodCollection
    {
        protected DapperMethodCollection(IReadOnlyList<IDapperMethodInfo> methods)
        {
            _methods = methods;
        }

        private readonly IReadOnlyList<IDapperMethodInfo> _methods;

        protected static MethodInfo GetMethod<TOut>(Expression<Func<MockDatabase, TOut>> expression)
        {
            if (expression.Body is UnaryExpression unaryExpression)
            {
                var unaryExpressionOperand = (MethodCallExpression)unaryExpression.Operand;
                return unaryExpressionOperand.Method;
            }

            var methodCallExpression = (MethodCallExpression)expression.Body;
            return methodCallExpression.Method;
        }

        internal IDapperMethodInfo GetMethodReference(MethodBase dapperMethod)
        {
            var matchedMethods = _methods.Where(methodInfo => methodInfo.MatchesDapperMethod(dapperMethod)).ToArray();

            if (!matchedMethods.Any())
                return ThrowMethodInfoNotFound(dapperMethod);

            if (matchedMethods.Length == 1)
                return matchedMethods.Single();

            throw new InvalidOperationException($"Multiple methods ({matchedMethods.Length}) found to match:\r\n{string.Join("\r\n", (IEnumerable<IDapperMethodInfo>)matchedMethods)}");
        }

        private IDapperMethodInfo ThrowMethodInfoNotFound(MethodBase dapperMethod)
        {
            var stack = new StackTrace();
            var methodsThatMatchByName = _methods.Where(m => m.ToString().StartsWith(dapperMethod.Name)).ToArray();
            var methodUnmatchDetails = methodsThatMatchByName.Select(m => m.GetMatchesDapperMethodReasons(dapperMethod));

            throw new ArgumentOutOfRangeException(
                nameof(dapperMethod), 
                $@"Unable to find method `{dapperMethod}`
Candidate methods ({methodsThatMatchByName.Length} like ^{dapperMethod.Name}.*):
{string.Join("\r\n", methodsThatMatchByName.Select(m => m.ToString()))}

Reasons for not matching:
{string.Join("\r\n", methodUnmatchDetails)}

All possible methods:
{string.Join("\r\n", _methods.Select(m => m.ToString()))}

Stack:
{string.Join("", stack.GetFrames().Take(10))}

");
        }
    }
}
