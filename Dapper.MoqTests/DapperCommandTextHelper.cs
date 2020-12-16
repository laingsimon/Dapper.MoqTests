using System.Text.RegularExpressions;

namespace Dapper.MoqTests
{
    public class DapperCommandTextHelper : IDapperCommandTextHelper
    {
        public string ConvertDapperParametersToUserParameters(string commandText)
        {
            //replace any "(@XX1,@XX2[,...])" with @XX

            var matches = Regex.Matches(commandText, @"(\(\@.+?\))");

            foreach (Match match in matches)
            {
                var matched = match.Value;
                var parameterName = Regex.Match(matched, @"\((\@.+?)\d+\,?");

                if (parameterName.Success)
                {
                    commandText = commandText.Replace(matched, parameterName.Groups[1].Value);
                }
            }

            return commandText;
        }
    }
}
