using System.Reflection;

namespace Dapper.MoqTests
{
    public interface IDapperMethodInfo
    {
        MethodInfo GetDapperMethod(params System.Type[] types);
        bool MatchesDapperMethod(System.Reflection.MethodBase dapperMethod);
        string GetMatchesDapperMethodReasons(MethodBase dapperMethod);
    }
}
