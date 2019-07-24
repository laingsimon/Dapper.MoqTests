using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.MoqTests
{
    public abstract partial class MockDatabase
    {
        /// <summary>
        /// Perform a asynchronous multi-mapping query with 3 input types. This returns a
        /// single type, combined from the raw types via map.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.Map)] Func<TFirst, TSecond, TThird, TReturn> map,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.Buffered)] bool buffered = true,
            [ParameterType(ParameterType.SplitOn)] string splitOn = "Id",
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a query asynchronously using .NET 4.5 Task.
        /// </summary>
        public abstract Task<IEnumerable<T>> QueryAsync<T>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Perform a asynchronous multi-mapping query with 4 input types. This returns a
        /// single type, combined from the raw types via map.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.Map)] Func<TFirst, TSecond, TThird, TFourth, TReturn> map,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.Buffered)] bool buffered = true,
            [ParameterType(ParameterType.SplitOn)] string splitOn = "Id",
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Perform a asynchronous multi-mapping query with 5 input types. This returns a
        /// single type, combined from the raw types via map.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.Map)] Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.Buffered)] bool buffered = true,
            [ParameterType(ParameterType.SplitOn)] string splitOn = "Id",
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Perform a asynchronous multi-mapping query with 7 input types. This returns a
        /// single type, combined from the raw types via map.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.Map)] Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.Buffered)] bool buffered = true,
            [ParameterType(ParameterType.SplitOn)] string splitOn = "Id",
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a query asynchronously using .NET 4.5 Task.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<object>> QueryAsync(Type type,
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Execute a query asynchronously using .NET 4.5 Task.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<dynamic>> QueryAsync(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Perform a asynchronous multi-mapping query with an arbitrary number of input
        /// types. This returns a single type, combined from the raw types via map.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<TReturn>> QueryAsync<TReturn>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.Types)] Type[] types,
            [ParameterType(ParameterType.Map)] Func<object[], TReturn> map,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.Buffered)] bool buffered = true,
            [ParameterType(ParameterType.SplitOn)] string splitOn = "Id",
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Perform a asynchronous multi-mapping query with 6 input types. This returns a
        /// single type, combined from the raw types via map.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.Map)] Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.Buffered)] bool buffered = true,
            [ParameterType(ParameterType.SplitOn)] string splitOn = "Id",
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

        /// <summary>
        /// Perform a asynchronous multi-mapping query with 2 input types. This returns a
        /// single type, combined from the raw types via map.
        /// </summary>
        [Obsolete(NotSupported)]
        public abstract Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
            [ParameterType(ParameterType.SqlText)] string sql,
            [ParameterType(ParameterType.Map)] Func<TFirst, TSecond, TReturn> map,
            [ParameterType(ParameterType.SqlParameters)] object param = null,
            [ParameterType(ParameterType.SqlTransaction)] IDbTransaction transaction = null,
            [ParameterType(ParameterType.Buffered)] bool buffered = true,
            [ParameterType(ParameterType.SplitOn)] string splitOn = "Id",
            [ParameterType(ParameterType.CommandTimeout)] int? commandTimeout = null,
            [ParameterType(ParameterType.CommandType)] CommandType? commandType = null);

    }
}
