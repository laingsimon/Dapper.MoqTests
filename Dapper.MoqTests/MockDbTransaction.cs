using System.Data;
using System.Data.Common;

namespace Dapper.MoqTests
{
    public class MockDbTransaction : DbTransaction
    {
        public override IsolationLevel IsolationLevel { get; }
        protected override DbConnection DbConnection { get; }

        public MockDbTransaction(DbConnection connection, IsolationLevel il = default)
        {
            DbConnection = connection;
            IsolationLevel = il;
        }

        public MockDbTransaction()
        { }

        public override void Commit()
        { }

        public override void Rollback()
        { }
    }
}
