namespace Dapper.MoqTests
{
    using System;
    using System.Diagnostics;
    using System.Text;

    internal class SqlText : IEquatable<SqlText>
    {
        internal static readonly SqlText Any = new SqlText(MockDbConnection.Any);

        private readonly string sql;
        private readonly string originalSql;

        [DebuggerStepThrough]
        public SqlText(string sql)
        {
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

        public static implicit operator SqlText(string sql)
        {
            return new SqlText(sql);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(sql);
        }

        public override string ToString()
        {
            if (ReferenceEquals(this, Any))
                return "<ANY>";

            return originalSql;
        }

        public static SqlText Create(string sql)
        {
            return sql == MockDbConnection.Any
                ? Any
                : new SqlText(sql);
        }
    }
}