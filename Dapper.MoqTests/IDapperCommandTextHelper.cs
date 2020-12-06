namespace Dapper.MoqTests
{
    public interface IDapperCommandTextHelper
    {
        string ConvertDapperParametersToUserParameters(string commandText);
    }
}
