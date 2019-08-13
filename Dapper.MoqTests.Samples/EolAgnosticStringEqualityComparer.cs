using System.Collections.Generic;

namespace Dapper.MoqTests.Samples
{
    internal class EolAgnosticStringEqualityComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            return GetComparableString(x).Equals(GetComparableString(y));
        }

        public int GetHashCode(string obj)
        {
            return GetComparableString(obj)?.GetHashCode() ?? 0;
        }

        private string GetComparableString(string text)
        {
            return text?.Replace("\r\n", "\n");
        }
    }
}
