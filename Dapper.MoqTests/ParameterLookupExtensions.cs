using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal static class ParameterLookupExtensions
    {
        public static object[] GetValues(this MethodInfo method, IReadOnlyDictionary<ParameterType, object> lookup, object defaultValue = null)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return (from param in method.GetParameters()
                select lookup.GetValue(param, defaultValue ?? param.DefaultValue)).ToArray();
        }

        private static object GetValue(this IReadOnlyDictionary<ParameterType, object> parametersLookup, ParameterInfo parameter, object defaultValue = null)
        {
            var paramType = parameter.GetCustomAttributeFromSelfOrInterface<ParameterTypeAttribute>();

            if (paramType == null || !parametersLookup.ContainsKey(paramType.Type))
            {
                Debug.WriteLine($"Unable to find value for parameter {parameter.Name} ({parameter.ParameterType})");
                return defaultValue;
            }

            return parametersLookup[paramType.Type];
        }

        private static T GetCustomAttributeFromSelfOrInterface<T>(this ParameterInfo parameter)
            where T: Attribute
        {
            var attribute = parameter.GetCustomAttribute<T>();
            if (attribute != null)
                return attribute;

            var declaringMember = parameter.Member;
            if (declaringMember.MemberType != MemberTypes.Method)
                return null; //not possible to inspect parameters of properties, constructors, etc.

            var declaringMethod = (MethodInfo) declaringMember;
            var declaringType = declaringMember.DeclaringType;
            if (declaringType == null)
                return null;

            var implementedInterfaces = declaringType.GetInterfaces();

            if (!implementedInterfaces.Any())
                return null;

            var methodFromInterface = (
                from interfaceType in implementedInterfaces
                let interfaceMethod = interfaceType.GetMethodThatMatches(declaringMethod)
                where interfaceMethod != null
                select interfaceMethod).SingleOrDefault();

            if (methodFromInterface == null)
                return null;

            var parameterFromInterfaceMember = methodFromInterface.GetParameters()
                .SingleOrDefault(p => p.Name == parameter.Name && p.ParameterType == parameter.ParameterType);

            return parameterFromInterfaceMember?.GetCustomAttribute<T>();
        }

        private static MethodInfo GetMethodThatMatches(this Type type, MethodInfo referenceMethod)
        {
            var referenceMethodParameterTypes = referenceMethod.GetParameters().Select(p => p.ParameterType).ToArray();

            return (from method in type.GetMethods()
                let parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray()
                where method.Name == referenceMethod.Name
                      && method.ReturnType.EqualsReturnType(referenceMethod.ReturnType)
                      && parameterTypes.SequenceEqual(referenceMethodParameterTypes)
                select method).SingleOrDefault();
        }

        private static bool EqualsReturnType(this Type checkMethodReturnType, Type referenceMethodReturnType)
        {
            if (checkMethodReturnType == referenceMethodReturnType)
                return true;

            return referenceMethodReturnType.IsGenericType
                && checkMethodReturnType.IsGenericType
                && checkMethodReturnType.GetGenericTypeDefinition() == referenceMethodReturnType.GetGenericTypeDefinition();
        }
    }
}
