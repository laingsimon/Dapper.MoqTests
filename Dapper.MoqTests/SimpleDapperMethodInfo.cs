using System;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal class SimpleDapperMethodInfo : IDapperMethodInfo
    {
        private readonly string methodName;
        private readonly MethodInfo concreteDapperMethod;
        private readonly MethodInfo methodToInvoke;

        public SimpleDapperMethodInfo(MethodInfo concreteDapperMethod)
            :this(null, concreteDapperMethod) { }

        public SimpleDapperMethodInfo(string methodName, MethodInfo concreteDapperMethod, MethodInfo methodToInvoke = null)
        {
            this.concreteDapperMethod = concreteDapperMethod;
            this.methodToInvoke = methodToInvoke ?? concreteDapperMethod;
            this.methodName = methodName ?? concreteDapperMethod.Name;
        }

        public override string ToString()
        {
            return $"{methodName} -> {concreteDapperMethod.Name}";
        }

        public bool MatchesDapperMethod(MethodBase dapperMethod)
        {
            return GetMatchesDapperMethodReasons(dapperMethod) == null;
        }

        public MethodInfo GetDapperMethod(params Type[] types)
        {
            if (types.Any())
            {
                throw new InvalidOperationException("Non-Generic methods cannot contain type arguments");
            }

            return methodToInvoke;
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

        public string GetMatchesDapperMethodReasons(MethodBase dapperMethod)
        {
            if (dapperMethod.IsGenericMethod)
                return "Mustn't be a generic method";

            var dapperMethodParameters = dapperMethod.GetParameters().Skip(1).ToArray(); //first argument is the extension method parameter; IDbConnection

            if (dapperMethod.Name != methodName
                || dapperMethodParameters.Length != concreteDapperMethod.GetParameters().Length)
            {
                return $"Method name (`{dapperMethod.Name}` vs `{methodName}`) or number of parameters ({dapperMethodParameters.Length} vs {concreteDapperMethod.GetParameters().Length}) doesn't match";
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
