using System;
using System.Text;

namespace Dapper.MoqTests
{
    internal class SqlText : IEquatable<SqlText>
    {
        internal static readonly string Any = "<ANY>:" + Guid.NewGuid();

        private readonly string _sql;
        private readonly string _originalSql;

        public SqlText(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException(nameof(sql));

            _sql = GetComparableText(sql);
            _originalSql = sql;
        }

        public bool Equals(SqlText other)
        {
            return other != null && _sql.Equals(other._sql, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SqlText);
        }

        private static string GetComparableText(string sql)
        {
            var builder = new StringBuilder();

            foreach (var line in sql.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Trim().Length > 0)
                    builder.AppendLine(line.Trim());
            }

            return builder.ToString();
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(_sql);
        }

        public override string ToString()
        {
            return _originalSql;
        }
    }
}