using System;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal class GenericDapperMethodInfo : IDapperMethodInfo
    {
        private readonly string methodName;
        private readonly MethodInfo concreteDapperMethod;

        public GenericDapperMethodInfo(MethodInfo concreteDapperMethod)
        {
            this.methodName = concreteDapperMethod.Name;
            this.concreteDapperMethod = concreteDapperMethod;
        }

        public override string ToString()
        {
            return methodName;
        }

        public bool MatchesDapperMethod(MethodBase dapperMethod)
        {
            if (!dapperMethod.IsGenericMethod)
                return false;

            var dapperMethodParameters = dapperMethod.GetParameters().Skip(1).ToArray(); //first argument is the extension method parameter; IDbConnection

            if (dapperMethod.Name != methodName 
                || dapperMethod.GetGenericArguments().Length != concreteDapperMethod.GetGenericArguments().Length 
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

        public MethodInfo GetDapperMethod(params Type[] dataTypes)
        {
            var genericMethodDefinition = concreteDapperMethod.GetGenericMethodDefinition();

            if (genericMethodDefinition.GetGenericArguments().Length != dataTypes.Length)
                throw new InvalidOperationException($"Generic method requires {genericMethodDefinition.GetGenericArguments().Length} types, {dataTypes.Length} was provided");

            return genericMethodDefinition.MakeGenericMethod(dataTypes);
        }
    }
}
