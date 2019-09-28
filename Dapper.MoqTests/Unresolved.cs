namespace Dapper.MoqTests
{
    public class Unresolved
    {
        /// <summary>
        /// If the `buffered` parameter cannot be detected, use this value
        /// </summary>
        public bool Buffered { get; set; } = true;

        /// <summary>
        /// Use this value for the SplitOn parameter when it cannot be resolved
        /// </summary>
        public string SplitOn { get; set; } = "Id";
    }
}
