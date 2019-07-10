using System.Data;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Dapper.MoqTests.Samples
{
    [TestFixture]
    public class FailureMessages
    {
        [Test]
        public void IncorrectSqlText()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            repository.GetCars();

            //NOTE: As there is no setup, you must use <object> in the verify
            try
            {
                connection.Verify(c => c.Query<object>(@"incorrect sql", It.IsAny<object>(), It.IsAny<IDbTransaction>()));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.Query<Object>(Match.Create<String>(, () => ""incorrect sql""), It.IsAny<Object>(), It.IsAny<IDbTransaction>())
No setups configured.

Performed invocations:
MockDatabase.Query<Object>(""select * 
from [Cars] 
order by Make, Model"", <No command parameters>, null)"));
            }
        }

        [Test]
        public async Task IncorrectParameterValue()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetCarAsync("reg");

            //NOTE: As there is no setup, you must use <object> in the verify
            try
            {
                connection.Verify(c => c.QueryAsync<object>(@"select * 
from [Cars] 
where Registration = @registration", new { registration = "incorrect value" }, It.IsAny<IDbTransaction>()));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.QueryAsync<Object>(Match.Create<String>(, () => ""select * 
from [Cars] 
where Registration = @registration""), Match.Create<Object>(, () => { registration = incorrect value }), It.IsAny<IDbTransaction>())
No setups configured.

Performed invocations:
MockDatabase.QueryAsync<Object>(""select * 
from [Cars] 
where Registration = @registration"", { registration = reg }, null)"));
            }
        }

        [Test]
        public async Task IncorrectParameterType()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetCarAsync("reg");

            //NOTE: As there is no setup, you must use <object> in the verify
            try
            {
                connection.Verify(c => c.QueryAsync<object>(@"select * 
from [Cars] 
where Registration = @registration", new { registration = 1 }, It.IsAny<IDbTransaction>()));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.QueryAsync<Object>(Match.Create<String>(, () => ""select * 
from [Cars] 
where Registration = @registration""), Match.Create<Object>(, () => { registration = 1 }), It.IsAny<IDbTransaction>())
No setups configured.

Performed invocations:
MockDatabase.QueryAsync<Object>(""select * 
from [Cars] 
where Registration = @registration"", { registration = reg }, null)"));
            }
        }

        [Test]
        public async Task IncorrectParameterSet()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetCarAsync("reg");

            //NOTE: As there is no setup, you must use <object> in the verify
            try
            {
                connection.Verify(c => c.QueryAsync<object>(@"select * 
from [Cars] 
where Registration = @registration", new { foo = "bar" }, It.IsAny<IDbTransaction>()));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.QueryAsync<Object>(Match.Create<String>(, () => ""select * 
from [Cars] 
where Registration = @registration""), Match.Create<Object>(, () => { foo = bar }), It.IsAny<IDbTransaction>())
No setups configured.

Performed invocations:
MockDatabase.QueryAsync<Object>(""select * 
from [Cars] 
where Registration = @registration"", { registration = reg }, null)"));
            }
        }
    }
}
