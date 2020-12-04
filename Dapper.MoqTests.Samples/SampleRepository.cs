using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dapper.MoqTests.Samples
{
    public class SampleRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SampleRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Car> GetCars()
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return connection.Query<Car>(@"select * 
from [Cars] 
order by Make, Model");
            }
        }

        public Car GetCar(string registration)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return connection.QuerySingle<Car>(@"select * 
from [Cars] 
where Registration = @registration", new { registration });
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public async Task<Car> GetCarAsync(string registration)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return await connection.QuerySingleAsync<Car>(@"select * 
from [Cars] 
where Registration = @registration", new { registration });
            }
        }

        public async Task<int> GetCarCountAsync(string registration)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return await connection.QuerySingleAsync<int>(@"select * 
from [Cars] 
where Registration = @registration", new { registration });
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEnumerable<string> GetModels(string make)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return connection.Query<string>(@"select distinct Model 
from [Cars] 
where Make = @make
order by Model", new { make });
            }
        }

        public void DeleteCar(string registration)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                connection.Execute(@"delete from [Cars] 
where Registration = @registration", new { registration });
            }
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        internal async Task<IEnumerable<Car>> GetCarsAsync()
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return await connection.QueryAsync<Car>(@"select * 
from [Cars] 
order by Make, Model");
            }
        }

        public async Task DeleteCarAsync(string registration)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                var transaction = connection.BeginTransaction();
                await connection.ExecuteAsync(@"delete from [Cars] 
where Registration = @registration", new { registration }, transaction: transaction);
                transaction.Commit();
            }
        }

        public async Task GetModelsSinceAsync(string make, int sinceYear)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                await connection.QueryAsync<Car>(
                    @"sp_getModelsSince", 
                    new { make, sinceYear }, 
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 100);
            }
        }

        public async Task<int> GetModelsCountAsync(string make)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return await connection.ExecuteScalarAsync<int>(
                    @"select count(distinct Model) from [Cars] where Make = @make",
                    new { make });
            }
        }

        public object GetModelsCount(string make)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return connection.ExecuteScalar(
                    @"select count(distinct Model) from [Cars] where Make = @make",
                    new { make });
            }
        }

        public IEnumerable<object> GetCarsAndMakes()
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return connection.Query<int, int, object>(
                    @"select count(*) from [Cars]
                      select count(distinct Make) from [Cars]",
                    (cars, makes) => new { cars, makes }).ToList();
            }
        }

        public async Task<bool> AnyCarsAsync(CancellationToken cancellation)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                return (await connection.QueryAsync<int>(new CommandDefinition(
                    "select count(*) from [Cars]",
                    cancellationToken: cancellation
                    ))).Any();
            }
        }

        public async Task DeleteCarsAsync(int[] ids)
        {
            using (var connection = _connectionFactory.OpenConnection())
            {
                await connection.ExecuteAsync(
                    "delete from [Cars] where Id = @ids",
                    new { ids });
            }
        }
    }
}
