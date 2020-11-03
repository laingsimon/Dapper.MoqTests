using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal abstract partial class SqlMapper : MockDatabase
    {
        protected static Type String = typeof(string);
        protected static Type Object = typeof(object);
        protected static Type Transaction = typeof(IDbTransaction);
        protected static Type CommandDefinition = typeof(CommandDefinition);

        [Obsolete("Not for use, only to support with reflection")]
        public SqlMapper(Moq.MockBehavior behaviour, Settings settings)
            :base(behaviour, settings)
        {
            throw new NotSupportedException("Not for use, only to support with reflection");
        }

        protected static Type Nullable<T>()
            where T: struct
        {
            return typeof(T?);
        }

        protected static MethodInfo Method(string name, params Type[] types)
        {
            var overloads = typeof(Dapper.SqlMapper)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == name && m.GetGenericArguments().Length == 0)
                .ToArray();

            return overloads
                .Where(IsAnExtensionMethod)
                .Single(m => ParametersMatch(m, types));
        }

        protected static MethodInfo Method<T>(string name, params Type[] types)
        {
            var overloads = typeof(Dapper.SqlMapper)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.Name == name && m.GetGenericArguments().Length == 1)
                .ToArray();

            return overloads
                .Where(IsAnExtensionMethod)
                .Single(m => ParametersMatch(m, types));
        }

        private static bool IsAnExtensionMethod(MethodInfo method)
        {
            return method.IsStatic
                && method.GetParameters().Length >= 1
                && method.GetParameters().First().ParameterType == typeof(IDbConnection);
        }

        private static bool ParametersMatch(MethodInfo method, Type[] expectedParameters)
        {
            var methodParameters = method.GetParameters().Skip(1).ToArray();

            return expectedParameters.Length == methodParameters.Length
                && expectedParameters.Zip(methodParameters, (expected, actual) =>
                new
                {
                    expected,
                    actual,
                    matches = expected == actual.ParameterType
                }).All(a => a.matches);
        }
    }
}
