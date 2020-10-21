using System.Collections.Generic;
using System.Data;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Dapper.MoqTests.Samples
{
    [TestFixture]
    public class FailureMessages
    {
        private readonly IEqualityComparer<string> _comparer = new EolAgnosticStringEqualityComparer();

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

            try
            {
                connection.Verify(c => c.Query<Car>(@"incorrect sql", It.IsAny<object>(), It.IsAny<IDbTransaction>(), true, null, null));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.Query<Car>(Match.Create<string>(Predicate<string>, () => ""incorrect sql""), It.IsAny<object>(), It.IsAny<IDbTransaction>(), True, null, null)

Performed invocations:

   Mock<MockDatabase:4> (c):

      MockDatabase.Query<Car>(""select * 
from [Cars] 
order by Make, Model"", null, null, True, null, null)").Using(_comparer));
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

            try
            {
                connection.Verify(c => c.QuerySingleAsync<Car>(@"select * 
from [Cars] 
where Registration = @registration", new { registration = "incorrect value" }, It.IsAny<IDbTransaction>(), null, null));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.QuerySingleAsync<Car>(Match.Create<string>(Predicate<string>, () => ""select * 
from [Cars] 
where Registration = @registration""), Match.Create<object>(Predicate<object>, () => { registration = incorrect value }), It.IsAny<IDbTransaction>(), null, null)

Performed invocations:

   Mock<MockDatabase:3> (c):

      MockDatabase.QuerySingleAsync<Car>(""select * 
from [Cars] 
where Registration = @registration"", { registration = reg }, null, null, null)").Using(_comparer));
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

            try
            {
                connection.Verify(c => c.QuerySingleAsync<Car>(@"select * 
from [Cars] 
where Registration = @registration", new { registration = 1 }, It.IsAny<IDbTransaction>(), null, null));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.QuerySingleAsync<Car>(Match.Create<string>(Predicate<string>, () => ""select * 
from [Cars] 
where Registration = @registration""), Match.Create<object>(Predicate<object>, () => { registration = 1 }), It.IsAny<IDbTransaction>(), null, null)

Performed invocations:

   Mock<MockDatabase:2> (c):

      MockDatabase.QuerySingleAsync<Car>(""select * 
from [Cars] 
where Registration = @registration"", { registration = reg }, null, null, null)").Using(_comparer));
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

            try
            {
                connection.Verify(c => c.QuerySingleAsync<Car>(@"select * 
from [Cars] 
where Registration = @registration", new { foo = "bar" }, It.IsAny<IDbTransaction>(), null, null));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.QuerySingleAsync<Car>(Match.Create<string>(Predicate<string>, () => ""select * 
from [Cars] 
where Registration = @registration""), Match.Create<object>(Predicate<object>, () => { foo = bar }), It.IsAny<IDbTransaction>(), null, null)

Performed invocations:

   Mock<MockDatabase:1> (c):

      MockDatabase.QuerySingleAsync<Car>(""select * 
from [Cars] 
where Registration = @registration"", { registration = reg }, null, null, null)").Using(_comparer));
            }
        }

        [Test]
        public async Task VerifyGenericCallWithoutSetup()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetCarAsync("reg");

            try
            {
                connection.Verify(c => c.QuerySingleAsync<Car>(@"select * 
from [Cars] 
where Registration = @registration", It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null));
            }
            catch (MockException exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.QuerySingleAsync<Car>(Match.Create<string>(, () => ""select * 
from [Cars] 
where Registration = @registration""), It.IsAny<object>(), It.IsAny<IDbTransaction>())
No setups configured.

Performed invocations:

   Mock<MockDatabase:1> (c):

      MockDatabase.QuerySingleAsync<Car>(""select * 
from [Cars] 
where Registration = @registration"", { registration = reg }, null)").Using(_comparer));
            }
        }

        [Test]
        public async Task VerifyTwoDifferentGenericCallsWithSameSql()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetCarAsync("reg");
            Assert.That(
                async () => await repository.GetCarCountAsync("reg"),
                Throws.InvalidOperationException.And.Message.EqualTo(@"Unable to detect the required response type for the command, it could be one of 2 possible options.

Command: 'select * 
from [Cars] 
where Registration = @registration'
Parameters: `registration = reg`
CommandType: Text

To be able to Verify the Dapper call accurately the Command and Parameters (and return type) must be unique for every invocation of a Dapper method.

Possible options: `Dapper.MoqTests.Samples.Car`, `System.Int32`

If this issue cannot be resolved, consider setting `Dapper.MoqTests.Settings.ResetDapperCachePerCommand` to `true`, note this is not a thread-safe approach").Using(_comparer));

           connection.Verify(c => c.QuerySingleAsync<Car>(@"select *
from [Cars] 
where Registration = @registration", It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null));

            try
            {
                connection.Verify(c => c.QuerySingleAsync<int>(@"select *
from [Cars] 
where Registration = @registration", It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null));
            }
            catch (System.Exception exc)
            {
                Assert.That(exc.Message.Trim(), Is.EqualTo(@"Expected invocation on the mock at least once, but was never performed: c => c.QuerySingleAsync<int>(Match.Create<string>(Predicate<string>, () => ""select *
from [Cars] 
where Registration = @registration""), It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null)

Performed invocations:

   Mock<MockDatabase:6> (c):

      MockDatabase.QuerySingleAsync<Car>(""select * 
from [Cars] 
where Registration = @registration"", { registration = reg }, null, null, null)").Using(_comparer));
            }
        }
    }
}
