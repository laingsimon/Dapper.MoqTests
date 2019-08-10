using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.MoqTests
{
    public class SqlTextComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            var comparableX = GetComparableText(x);
            var comparableY = GetComparableText(y);
            return comparableX.Equals(comparableY, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
        }

        private static string GetComparableText(string sql)
        {
            var builder = new StringBuilder();

            foreach (var line in sql.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Trim().Length > 0)
                    builder.AppendLine(line.Trim());
            }

            return builder.ToString();
        }
    }
}
