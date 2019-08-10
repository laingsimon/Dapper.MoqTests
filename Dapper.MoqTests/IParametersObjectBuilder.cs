using System.Collections.Generic;
using System.Data.Common;

namespace Dapper.MoqTests
{
    public interface ISqlParametersBuilder
    {
        object FromParameters(IReadOnlyCollection<DbParameter> parameters);
    }
}
