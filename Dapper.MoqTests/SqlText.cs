namespace Dapper.MoqTests
{
    using System;
    using System.Text;

    internal class SqlText : IEquatable<SqlText>
    {
        internal static readonly string Any = "<ANY>:" + Guid.NewGuid();

        private readonly string sql;
        private readonly string originalSql;

        public SqlText(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException(nameof(sql));

            this.sql = GetComparableText(sql);
            this.originalSql = sql;
        }

        public bool Equals(SqlText other)
        {
            return other != null && sql.Equals(other.sql, StringComparison.OrdinalIgnoreCase);
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
            return StringComparer.OrdinalIgnoreCase.GetHashCode(sql);
        }

        public override string ToString()
        {
            return originalSql;
        }
    }
}