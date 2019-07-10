using System.Data;
using System.Data.Common;

namespace Dapper.MoqTests
{
    public class MockDbTransaction : DbTransaction
    {
        public override IsolationLevel IsolationLevel { get; }
        protected override DbConnection DbConnection { get; }

        public MockDbTransaction(MockDbConnection connection, IsolationLevel il = default(IsolationLevel))
        {
            //TODO: Set DbConnection...
            IsolationLevel = il;
        }

        public MockDbTransaction(DbConnection connection, IsolationLevel il = default(IsolationLevel))
        {
            DbConnection = connection;
            IsolationLevel = il;
        }

        public MockDbTransaction(DbConnection connection)
        {
            DbConnection = connection;
        }

        public MockDbTransaction()
        { }

        public override void Commit()
        { }

        public override void Rollback()
        { }
    }
}
