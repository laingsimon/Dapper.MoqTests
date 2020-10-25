using System;
using System.Linq;
using System.Reflection;

namespace Dapper.MoqTests
{
    internal static class DapperMethods
    {
        private static readonly DapperMethodCollection Methods = new DapperMethodCollection();

        internal static MethodInfo GetScalar(MethodBase dapperMethod)
        {
            var method = Methods.GetMethodReference(dapperMethod);

            return dapperMethod.IsGenericMethod
                ? method.GetDapperMethod(typeof(object))
                : method.GetDapperMethod();
        }

        public static MethodInfo GetExecuteMethod(MethodBase dapperMethod, Type dataType)
        {
            var method = Methods.GetMethodReference(dapperMethod);
            if (dataType == null)
                return method.GetDapperMethod();

            return method.GetDapperMethod(dataType);
        }

        public static MethodInfo GetQueryMethod(MethodBase dapperMethod, params Type[] dataTypes)
        {
            var method = Methods.GetMethodReference(dapperMethod);

            return method.GetDapperMethod(dataTypes);
        }

        internal static bool IsSingleResultMethod(MethodInfo method)
        {
            return method.Name.StartsWith("QuerySingle");
        }
    }
}
