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
                            matches = ParameterMatches(dapperMethodParameter.ParameterType, concreteMethodParameter),
                            dapperMethodParameter,
                            concreteMethodParameter
                        });

            return parameterMatchInfo.All(parameterTypeMatches => parameterTypeMatches.matches);
        }

        private static bool ParameterMatches(Type dapperParameterType, ParameterInfo concreteParameterInfo)
        {
            return dapperParameterType.Equals(concreteParameterInfo.ParameterType)
                || (concreteParameterInfo.GetCustomAttribute(typeof(ParameterTypeAttribute)) != null 
                    && IsFuncWithCorrectNumberOfTypeArguments(dapperParameterType, concreteParameterInfo.ParameterType));
        }

        private static bool IsFuncWithCorrectNumberOfTypeArguments(Type dapperParameterType, Type concreteParameterType)
        {
            return IsFunc(dapperParameterType)
                && IsFunc(concreteParameterType)
                && dapperParameterType.GetGenericArguments().Length == concreteParameterType.GetGenericArguments().Length;
        }

        private static bool IsFunc(Type parameterType)
        {
            return parameterType.Name.StartsWith("Func");
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
