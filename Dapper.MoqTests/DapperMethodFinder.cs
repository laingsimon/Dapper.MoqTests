using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    public class DapperMethodFinder
    {
        private readonly DapperMethodMap methodMap;

        public DapperMethodFinder(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            methodMap = new DapperMethodMap(settings);
        }

        public MethodInfo FindUserMethodFromMethodInCallStack(MethodBase dapperMethod, Type[] dataType)
        {
            var log = new StringWriter();

            var genericArguments = dataType.Any() == true
                ? dataType
                : dapperMethod.GetGenericArguments();

            var dapperMethodChainMethod = FindOverload(methodMap.GetType(), dapperMethod, genericArguments, false, log);
            if (dapperMethodChainMethod != null)
            {
                log.WriteLine($"Found dapper chain method {dapperMethodChainMethod} that represents {dapperMethod}");

                if (dapperMethodChainMethod.ContainsGenericParameters)
                    dapperMethodChainMethod = dapperMethodChainMethod.GetBaseDefinition().MakeGenericMethod(dapperMethodChainMethod.GetGenericArguments().Select(ga => typeof(object)).ToArray());

                var dapperMethodChain = (DapperMethodChain)dapperMethodChainMethod.Invoke(methodMap, new object[0]);

                return dapperMethodChain.FindMethod(
                    dapperMethod,
                    dataType ?? new Type[0]);
            }

            var mockDatabaseMethod = FindOverload(typeof(MockDatabase), dapperMethod, genericArguments, true, log);
            if (mockDatabaseMethod != null)
            {
                return mockDatabaseMethod;
            }

            throw new NotSupportedException($"Could not find method map for {dapperMethod}\r\n{log.GetStringBuilder()}");
        }

        private static MethodInfo FindOverload(Type type, MethodBase dapperMethod, Type[] genericTypes, bool compareParameters, TextWriter log)
        {
            var dapperMethodChainMethods = type.GetMethods()
                .Where(m => m.Name == dapperMethod.Name)
                .Where(m => !compareParameters || HasCorrectNumberOfParameters(m, dapperMethod))
                .Where(m => m.GetGenericArguments().Length == genericTypes.Length)
                .ToArray();

            log.WriteLine($"Found {dapperMethodChainMethods.Length} method/s in {type.Name} that match {dapperMethod} (compareParameters: {compareParameters})");

            return FindOverload(dapperMethodChainMethods, dapperMethod, genericTypes, compareParameters, log);
        }

        private static bool HasCorrectNumberOfParameters(MethodInfo method, MethodBase dapperMethod)
        {
            if (IsExtensionMethod(dapperMethod))
            {
                return method.GetParameters().Length == dapperMethod.GetParameters().Length - 1;
            }

            return method.GetParameters().Length == dapperMethod.GetParameters().Length;
        }

        private static bool IsExtensionMethod(MethodBase dapperMethod)
        {
            return dapperMethod.IsStatic
                && dapperMethod.GetParameters().Length >= 1
                && dapperMethod.GetParameters()[0].ParameterType == typeof(IDbConnection);
        }

        private static MethodInfo FindOverload(ICollection<MethodInfo> overloads, MethodBase dapperMethod, Type[] genericTypes, bool compareParameters, TextWriter log)
        {
            var relevantDapperMethodParameters = IsExtensionMethod(dapperMethod)
                ? dapperMethod.GetParameters().Skip(1)
                : dapperMethod.GetParameters();
            var dapperMethodParameters = relevantDapperMethodParameters
                .Select(p => p.ParameterType)
                .ToArray();

            var matchingOverloads = overloads.Where(
                overload =>
                {
                    if (!compareParameters)
                        return true;

                    var overloadParameters = overload.GetParameters().Select(p => p.ParameterType).ToArray();
                    return overloadParameters.SequenceEqual(dapperMethodParameters);
                }).ToArray();

            log.WriteLine($"Found {matchingOverloads.Length} overload/s that match");

            if (matchingOverloads.Length == 0)
            {
                return null;
            }

            if (matchingOverloads.Length == 1)
            {
                var matchingOverload = matchingOverloads.First();

                return genericTypes.Length >= 1
                    ? matchingOverload.MakeGenericMethod(genericTypes)
                    : matchingOverload;
            }

            throw new InvalidOperationException($"Found {matchingOverloads.Length} matching overloads for {dapperMethod}\r\n{string.Join("\r\n", matchingOverloads.Select(m => m.ToString()))}");
        }
    }
}
