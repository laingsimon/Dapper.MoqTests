using System.Collections.Generic;

namespace Dapper.MoqTests
{
    public interface IParametersComparer : IEqualityComparer<object>, IEqualityComparer<string>
    {

    }
}
