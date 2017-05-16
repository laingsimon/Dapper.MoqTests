namespace Dapper.MoqTests
{
    using System.Reflection;
    using Moq;

    internal class MatchProxy : IMatch
    {
        private static readonly MethodInfo matches =
            typeof(Match).GetMethod("Matches", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly Match underlying;

        public MatchProxy(Match underlying)
        {
            this.underlying = underlying;
        }

        public bool Matches(object value)
        {
            return (bool)matches.Invoke(underlying, new[] {value});
        }
    }
}