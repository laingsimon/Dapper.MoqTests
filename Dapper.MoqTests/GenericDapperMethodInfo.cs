using System;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal class GenericDapperMethodInfo : IDapperMethodInfo
    {
        private readonly string methodName;
        private readonly MethodInfo concreteDapperMethod;
        private readonly MethodInfo methodToInvoke;

        public GenericDapperMethodInfo(MethodInfo concreteDapperMethod)
            :this(null, concreteDapperMethod)
        { }

        public GenericDapperMethodInfo(string methodNameOverride, MethodInfo concreteDapperMethod, MethodInfo methodToInvoke = null)
        {
            this.methodName = methodNameOverride ?? concreteDapperMethod.Name;
            this.concreteDapperMethod = concreteDapperMethod;
            this.methodToInvoke = methodToInvoke ?? concreteDapperMethod;
        }

        public override string ToString()
        {
            return $"{methodName} --> {concreteDapperMethod.Name} (Generic)";
        }

        public bool MatchesDapperMethod(MethodBase dapperMethod)
        {
            return GetMatchesDapperMethodReasons(dapperMethod) == null;
        }

        private static bool ParameterMatches(Type dapperParameterType, ParameterInfo concreteParameterInfo)
        {
            return dapperParameterType.Equals(concreteParameterInfo.ParameterType)
                || (concreteParameterInfo.GetCustomAttribute(typeof(ParameterTypeAttribute)) != null 
                    && IsFuncWithCorrectNumberOfTypeArguments(dapperParameterType, concreteParameterInfo.ParameterType))
                || (ByRefParameterTypeMatches(dapperParameterType, concreteParameterInfo.ParameterType));
        }

        private static bool ByRefParameterTypeMatches(Type dapperParameterType, Type parameterType)
        {
            if (dapperParameterType.IsByRef && !parameterType.IsByRef)
            {
                return parameterType.MakeByRefType().Equals(dapperParameterType);
            }

            return false;
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
            if (methodToInvoke.GetGenericArguments().Length == 0)
            {
                return methodToInvoke;
            }

            var genericMethodDefinition = methodToInvoke.GetGenericMethodDefinition();

            if (genericMethodDefinition.GetGenericArguments().Length != dataTypes.Length)
                throw new InvalidOperationException($"Generic method requires {genericMethodDefinition.GetGenericArguments().Length} types, {dataTypes.Length} was provided");

            return genericMethodDefinition.MakeGenericMethod(dataTypes);
        }

        public string GetMatchesDapperMethodReasons(MethodBase dapperMethod)
        {
            if (!dapperMethod.IsGenericMethod)
                return "Must be a generic method";

            var dapperMethodParameters = dapperMethod.GetParameters().Skip(1).ToArray(); //first argument is the extension method parameter; IDbConnection

            if (dapperMethod.Name != methodName
                || dapperMethod.GetGenericArguments().Length != concreteDapperMethod.GetGenericArguments().Length
                || dapperMethodParameters.Length != concreteDapperMethod.GetParameters().Length)
            {
                return $"Method name ({dapperMethod.Name} vs {methodName}), number of generic arguments ({dapperMethod.GetGenericArguments().Length} vs {concreteDapperMethod.GetGenericArguments().Length}) or number of parameters ({dapperMethodParameters.Length} vs {concreteDapperMethod.GetParameters().Length}) differ";
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

            var mismatchedParameters = parameterMatchInfo.Where(param => !param.matches).ToArray();
            var mismatchedParameterInfo = string.Join(", ", mismatchedParameters.Select(p => $"{p.dapperMethodParameter.ParameterType} vs {p.concreteMethodParameter.ParameterType}"));
            return !mismatchedParameters.Any()
                ? null
                : $"{mismatchedParameters.Length} of {dapperMethodParameters.Length} parameters do not match by type: {mismatchedParameterInfo}";
        }
    }
}
