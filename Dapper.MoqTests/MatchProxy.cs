using System.Reflection;
using Moq;

namespace Dapper.MoqTests
{
    internal class MatchProxy : IMatch
    {
        private static readonly MethodInfo MatchesMethod =
            typeof(Match).GetMethod("Matches", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly Match _underlying;

        public MatchProxy(Match underlying)
        {
            _underlying = underlying;
        }

        public bool Matches(object value)
        {
            return (bool)MatchesMethod.Invoke(_underlying, new[] {value});
        }
    }
}