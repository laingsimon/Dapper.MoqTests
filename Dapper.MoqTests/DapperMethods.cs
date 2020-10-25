using System;
using System.Reflection;

namespace Dapper.MoqTests
{
    public class DapperMethods
    {
        private readonly DapperMethodCollection methods;

        public DapperMethods(DapperMethodCollection methods)
        {
            this.methods = methods;
        }

        internal MethodInfo GetScalar(MethodBase dapperMethod)
        {
            var method = methods.GetMethodReference(dapperMethod);

            return dapperMethod.IsGenericMethod
                ? method.GetDapperMethod(typeof(object))
                : method.GetDapperMethod();
        }

        public MethodInfo GetExecuteMethod(MethodBase dapperMethod, Type dataType)
        {
            var method = methods.GetMethodReference(dapperMethod);
            if (dataType == null)
                return method.GetDapperMethod();

            return method.GetDapperMethod(dataType);
        }

        public MethodInfo GetQueryMethod(MethodBase dapperMethod, params Type[] dataTypes)
        {
            var method = methods.GetMethodReference(dapperMethod);

            return method.GetDapperMethod(dataTypes);
        }

        internal bool IsSingleResultMethod(MethodInfo method)
        {
            return method.Name.StartsWith("QuerySingle");
        }
    }
}
