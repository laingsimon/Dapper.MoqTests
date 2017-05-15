namespace Dapper.MoqTests
{
    using System.Data;

    internal class GenericMockDatabase<TReturnType> : IGenericMockDatabase
    {
        private readonly IMockDatabase underlyingDatabase;

        public GenericMockDatabase(IMockDatabase underlyingDatabase)
        {
            this.underlyingDatabase = underlyingDatabase;
        }

        public IDataReader Query(string text, object parameters)
        {
            return underlyingDatabase.Query<TReturnType>(text, parameters);
        }

        public object QuerySingle(string text, object parameters)
        {
            return underlyingDatabase.QuerySingle<TReturnType>(text, parameters);
        }
    }
}
