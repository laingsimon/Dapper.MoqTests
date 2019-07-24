namespace Dapper.MoqTests
{
    public enum ParameterType
    {
        /// <summary>
        /// The parameters for the Dapper command
        /// </summary>
        SqlParameters,

        /// <summary>
        /// The text for the Dapper command
        /// </summary>
        SqlText,

        /// <summary>
        /// The transaction for the Dapper command
        /// </summary>
        SqlTransaction,

        /// <summary>
        /// The timeout for the Dapper command
        /// </summary>
        CommandTimeout,

        /// <summary>
        /// The type of the Dapper command
        /// </summary>
        CommandType,

        /// <summary>
        /// Whether the command should be buffered
        /// </summary>
        Buffered,
        Map,
        SplitOn,
        Type,
        Types
    }
}