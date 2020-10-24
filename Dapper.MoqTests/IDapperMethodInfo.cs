using System.Reflection;

namespace Dapper.MoqTests
{
    internal interface IDapperMethodInfo
    {
        MethodInfo GetDapperMethod(params System.Type[] types);
        bool MatchesDapperMethod(System.Reflection.MethodBase dapperMethod);
    }
}
