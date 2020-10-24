using System.Data;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace Dapper.MoqTests.Samples
{
    [TestFixture]
    public class Samples
    {
        [SetUp]
        public void BeforeEachTest()
        {
            SqlMapper.PurgeQueryCache();
        }

        [Test]
        public void VerifyOnly()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            repository.GetCars();

            connection.Verify(c => c.Query<Car>(@"select *
from [Cars]
order by Make, Model", It.IsAny<object>(), It.IsAny<IDbTransaction>(), true, null, null));
        }

        [Test]
        public void VerifyQueryANumberOfTimes()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            repository.GetCars();

            connection.Verify(c => c.Query<Car>(@"select *
from [Cars]
order by Make, Model", It.IsAny<object>(), It.IsAny<IDbTransaction>(), true, null, null), Times.Once);
        }

        [Test]
        public async Task VerifyQueryAsync()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetCarsAsync();

            connection.Verify(c => c.QueryAsync<Car>(@"select *
from [Cars]
order by Make, Model", It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null), Times.Once);
        }

        [Test]
        public async Task VerifyQueryAsyncWithParameters()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetCarAsync("reg");

            connection.Verify(c => c.QuerySingleAsync<Car>(@"select * 
from [Cars] 
where Registration = @registration", new { registration = "reg" }, It.IsAny<IDbTransaction>(), null, null));
        }

        [Test]
        public void SetupOnly()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            var car = new Car
            {
                Registration = "ABC123",
                Make = "Vauxhall",
                Model = "Astra"
            };
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);
            connection.Setup(c => c.QuerySingle<Car>(@"select *
from [Cars]
where Registration = @registration", new { registration = "ABC123" }, It.IsAny<IDbTransaction>(), null, null))
                    .Returns(car);

            var result = repository.GetCar("ABC123");

            Assert.That(result.Make, Is.EqualTo("Vauxhall"));
            Assert.That(result.Model, Is.EqualTo("Astra"));
        }

        [Test]
        public void SetupAndVerify()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            var car = new Car
            {
                Registration = "ABC123",
                Make = "Vauxhall",
                Model = "Astra"
            };
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);
            connection.Setup(c => c.QuerySingle<Car>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null))
                    .Returns(car);

            repository.GetCar("ABC123");

            connection.Verify(c => c.QuerySingle<Car>(@"select *
from [Cars]
where Registration = @registration", new { registration = "ABC123" }, It.IsAny<IDbTransaction>(), null, null));
        }

        [Test]
        public void Execute()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            repository.DeleteCar("ABC123");

            connection.Verify(c => c.Execute(@"delete from [Cars]
where Registration = @registration", new { registration = "ABC123" }, null, null, null));
        }

        [Test]
        public async Task ExecuteAsync()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.DeleteCarAsync("ABC123");

            connection.Verify(c => c.ExecuteAsync(@"delete from [Cars]
where Registration = @registration", new { registration = "ABC123" }, It.IsAny<IDbTransaction>(), null, null));
        }

        [Test]
        public void ExecuteANumberOfTimes()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            repository.DeleteCar("ABC123");

            connection.Verify(c => c.Execute(@"delete from [Cars]
where Registration = @registration", new { registration = "ABC123" }, null, null, null), Times.Once);
        }

        [Test]
        public void ReturnManyComplexObjects()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            var vauxhall = new Car
            {
                Registration = "ABC123",
                Make = "Vauxhall",
                Model = "Astra"
            };
            var ford = new Car
            {
                Registration = "DEF456",
                Make = "Ford",
                Model = "Mondeo"
            };
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);
            connection.Setup(c => c.Query<Car>(@"select *
from [Cars]
order by Make, Model", It.IsAny<object>(), It.IsAny<IDbTransaction>(), true, null, null))
                    .Returns(new[] { vauxhall, ford });

            var result = repository.GetCars();

            Assert.That(result.Select(c => c.Model), Is.EquivalentTo(new[] { "Astra", "Mondeo" }));
        }

        [Test]
        public void SupportsMoqArgumentMatches()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);
            connection.Setup(c => c.Query<string>(It.Is<string>(sql => sql.Contains("[Cars]")), It.IsAny<object>(), It.IsAny<IDbTransaction>(), true, null, null))
                    .Returns(new[] { "Astra" });

            repository.GetModels("Vauxhall");

            connection.Verify(c => c.Query<string>(It.IsAny<string>(), It.Is<object>(p => p.Prop<string>("make") == "Vauxhall"), It.IsAny<IDbTransaction>(), true, null, null));
        }

        [Test]
        public async Task SupportsExpectedTransactionVerification()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            var transaction = new MockDbTransaction();
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);
            connection.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction);

            await repository.DeleteCarAsync("Vauxhall");

            connection.Verify(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), transaction, null, null));
        }

        [Test]
        public async Task SupportsTransactionVerificationWithoutSetup()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.DeleteCarAsync("Vauxhall");

            connection.Verify(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null));
        }

        [Test]
        public async Task VerifyTwoDifferentGenericCallsWithSameSqlWhenSettingEnabled()
        {
            var settings = new Settings
            {
                ResetDapperCachePerCommand = true
            };

            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection(settings);
            var repository = new SampleRepository(connectionFactory.Object);
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetCarAsync("reg");
            await repository.GetCarCountAsync("reg");

            connection.Verify(c => c.QuerySingleAsync<Car>(@"select *
from [Cars] 
where Registration = @registration", It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null));

            connection.Verify(c => c.QuerySingleAsync<int>(@"select *
from [Cars] 
where Registration = @registration", It.IsAny<object>(), It.IsAny<IDbTransaction>(), null, null));
        }

        [Test]
        public async Task SupportsStoredProcedures()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetModelsSinceAsync("Vauxhall", 2018);

            connection.Verify(c => c.QueryAsync<Car>("sp_getModelsSince", new { make = "Vauxhall", sinceYear = 2018 }, It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure));
        }

        [Test]
        public async Task SupportsCommandTimeout()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetModelsSinceAsync("Vauxhall", 2018);

            connection.Verify(c => c.QueryAsync<Car>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), 100, It.IsAny<CommandType?>()));
        }

        [Test]
        public async Task ExecuteScalarAsync()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.GetModelsCountAsync("Vauxhall");

            connection.Verify(c => c.ExecuteScalarAsync<int>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>()));
        }

        [Test]
        public void ExecuteScalar()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            repository.GetModelsCount("Vauxhall");

            connection.Verify(c => c.ExecuteScalar(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), It.IsAny<CommandType?>()));
        }

        [Test, Explicit]
        public void QueryMultiple()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            repository.GetCarsAndMakes();

            connection.Verify(c => c.Query(
                @"select count(*) from [Cars]
                  select count(distinct Make) from [Cars]",
                It.IsAny<Func<int, int, object>>(),
                It.IsAny<object>(),
                It.IsAny<IDbTransaction>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CommandType>()));
        }

        [Test]
        public async Task CommandDefinition()
        {
            var connectionFactory = new Mock<IDbConnectionFactory>();
            var connection = new MockDbConnection();
            var repository = new SampleRepository(connectionFactory.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancel = cancellationTokenSource.Token;

            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.AnyCarsAsync(cancel);

            connection.Verify(c => c.QueryAsync<int>(new CommandDefinition(
                "select count(*) from [Cars]",
                /*parameters*/ null,
                /*transaction*/ null,
                /*commandTimeout*/ null,
                /*commandType*/ null,
                /*flags*/ CommandFlags.Buffered,
                /*cancellationToken*/ cancel)));
        }
    }
}
