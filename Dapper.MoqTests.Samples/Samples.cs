using System.Data;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.MoqTests.Samples
{
    [TestFixture]
    public class Samples
    {
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

            //NOTE: As there is no setup, you must use <object> in the verify
            connection.Verify(c => c.Query<object>(@"select *
from [Cars]
order by Make, Model", It.IsAny<object>(), It.IsAny<IDbTransaction>()));
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

            //NOTE: As there is no setup, you must use <object> in the verify
            connection.Verify(c => c.Query<object>(@"select *
from [Cars]
order by Make, Model", It.IsAny<object>(), It.IsAny<IDbTransaction>()), Times.Once);
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

            //NOTE: As there is no setup, you must use <object> in the verify
            connection.Verify(c => c.QueryAsync<object>(@"select *
from [Cars]
order by Make, Model", It.IsAny<object>(), It.IsAny<IDbTransaction>()), Times.Once);
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

            //NOTE: As there is no setup, you must use <object> in the verify
            connection.Verify(c => c.QuerySingleAsync<object>(@"select * 
from [Cars] 
where Registration = @registration", new { registration = "reg" }, It.IsAny<IDbTransaction>()));
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
where Registration = @registration", new { registration = "ABC123" }, It.IsAny<IDbTransaction>()))
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
            connection.Setup(c => c.QuerySingle<Car>(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                    .Returns(car);

            repository.GetCar("ABC123");

            //NOTE: because the call to QuerySingle() was setup we should use the correct type-argument here
            connection.Verify(c => c.QuerySingle<Car>(@"select *
from [Cars]
where Registration = @registration", new { registration = "ABC123" }, It.IsAny<IDbTransaction>()));
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
where Registration = @registration", new { registration = "ABC123" }, null));
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
where Registration = @registration", new { registration = "ABC123" }, It.IsAny<IDbTransaction>()));
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
where Registration = @registration", new { registration = "ABC123" }, null), Times.Once);
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
order by Make, Model", It.IsAny<object>(), It.IsAny<IDbTransaction>()))
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
            var vauxhall = new Car
            {
                Registration = "ABC123",
                Make = "Vauxhall",
                Model = "Astra"
            };
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);
            connection.Setup(c => c.Query<Car>(It.Is<string>(sql => sql.Contains("[Cars]")), It.IsAny<object>(), It.IsAny<IDbTransaction>()))
                    .Returns(new[] { vauxhall });

            repository.GetModels("Vauxhall");

            connection.Verify(c => c.Query<Car>(It.IsAny<string>(), It.Is<object>(p => p.Prop<string>("make") == "Vauxhall"), It.IsAny<IDbTransaction>()));
        }

        [Test]
        public async Task SupportsExpectedTransactionVerification()
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
            var transaction = new MockDbTransaction();
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);
            connection.Setup(c => c.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction);

            await repository.DeleteCarAsync("Vauxhall");

            connection.Verify(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), transaction));
        }

        [Test]
        public async Task SupportsTransactionVerificationWithoutSetup()
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
            var transaction = new MockDbTransaction();
            connectionFactory
                .Setup(f => f.OpenConnection())
                .Returns(connection);

            await repository.DeleteCarAsync("Vauxhall");

            connection.Verify(c => c.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<IDbTransaction>()));
        }
    }
}
