using System;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal class SimpleDapperMethodInfo : IDapperMethodInfo
    {
        private readonly string methodName;
        private readonly MethodInfo concreteDapperMethod;

        public SimpleDapperMethodInfo(MethodInfo concreteDapperMethod)
            :this(null, concreteDapperMethod) { }

        public SimpleDapperMethodInfo(string methodName, MethodInfo concreteDapperMethod)
        {
            this.concreteDapperMethod = concreteDapperMethod;
            this.methodName = methodName ?? concreteDapperMethod.Name;
        }

        public override string ToString()
        {
            return methodName;
        }

        public bool MatchesDapperMethod(MethodBase dapperMethod)
        {
            if (dapperMethod.IsGenericMethod)
                return false;

            var dapperMethodParameters = dapperMethod.GetParameters().Skip(1).ToArray(); //first argument is the extension method parameter; IDbConnection

            if (dapperMethod.Name != this.methodName
                || dapperMethodParameters.Length != concreteDapperMethod.GetParameters().Length)
            {
                return false;
            }

            var parameterMatchInfo = dapperMethodParameters
                    .Zip(
                        concreteDapperMethod.GetParameters(),
                        (dapperMethodParameter, concreteMethodParameter) => new
                        {
                            matches = dapperMethodParameter.ParameterType.Equals(concreteMethodParameter.ParameterType),
                            dapperMethodParameter,
                            concreteMethodParameter
                        });

            return parameterMatchInfo.All(parameterTypeMatches => parameterTypeMatches.matches);
        }

        public MethodInfo GetDapperMethod(params Type[] types)
        {
            if (types.Any())
            {
                throw new InvalidOperationException("Non-Generic methods cannot contain type arguments");
            }

            return concreteDapperMethod;
        }
    }
}
