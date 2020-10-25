using System.Collections.Generic;

namespace Dapper.MoqTests
{
    public class ClassicDapperMethodCollection : DapperMethodCollection
    {
        private static readonly IReadOnlyList<IDapperMethodInfo> _classicMethods = new List<IDapperMethodInfo>
        {
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.Query(typeof(object), "some sql", null, null, true, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.Query<object>("some sql", null, null, true, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.QueryAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QueryAsync<object>("some sql", null, null, null, null))),

            new GenericDapperMethodInfo(GetMethod<object>(db => db.Query<object, object, object>("some sql", null, null, null, true, "Id", null, null))),

#if DOTNETFRAMEWORK && !DEBUG
            new SimpleDapperMethodInfo("ExecuteImpl", GetMethod<object>(db => db.Execute(new CommandDefinition())), GetMethod<object>(db => db.Execute("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.ExecuteAsync("some sql", null, null, null, null))),
#else
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.Execute("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.ExecuteAsync("some sql", null, null, null, null))),
#endif

            new SimpleDapperMethodInfo(GetMethod(db => db.QuerySingle(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QuerySingle<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.QuerySingleAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QuerySingleAsync<object>("some sql", null, null, null, null))),

            new SimpleDapperMethodInfo(GetMethod(db => db.QuerySingleOrDefault(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QuerySingleOrDefault<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod<object>(db => db.QuerySingleOrDefaultAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod<object>(db => db.QuerySingleOrDefaultAsync<object>("some sql", null, null, null, null))),

            new SimpleDapperMethodInfo(GetMethod(db => db.QueryFirst(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QueryFirst<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod(db => db.QueryFirstAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QueryFirstAsync<object>("some sql", null, null, null, null))),

            new SimpleDapperMethodInfo(GetMethod(db => db.QueryFirstOrDefault(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QueryFirstOrDefault<object>("some sql", null, null, null, null))),
            new SimpleDapperMethodInfo(GetMethod(db => db.QueryFirstOrDefaultAsync(typeof(object), "some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.QueryFirstOrDefaultAsync<object>("some sql", null, null, null, null))),

#if DOTNETFRAMEWORK && !DEBUG
            new GenericDapperMethodInfo("ExecuteScalarImpl", GetMethod(db => db.ExecuteScalar<object>(new CommandDefinition())), GetMethod(db => db.ExecuteScalar("some sql", null, null, null, null))),
#else
            new SimpleDapperMethodInfo("ExecuteScalar", GetMethod(db => db.ExecuteScalar("some sql", null, null, null, null))),
#endif
            new SimpleDapperMethodInfo(GetMethod(db => db.ExecuteScalarAsync("some sql", null, null, null, null))),
            new GenericDapperMethodInfo(GetMethod(db => db.ExecuteScalarAsync<object>("some sql", null, null, null, null)))
        };

        public ClassicDapperMethodCollection()
            :base(_classicMethods)
        { }
    }
}
