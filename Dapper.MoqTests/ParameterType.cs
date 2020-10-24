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

        /// <summary>
        /// The function to map row types to the return type.
        /// </summary>
        Map,

        /// <summary>
        /// The field we should split and read the second object from (default: "Id").
        /// </summary>
        SplitOn,

        /// <summary>
        /// The type to return.
        /// </summary>
        Type,

        /// <summary>
        /// Array of types in the recordset.
        /// </summary>
        Types,

        /// <summary>
        /// The command definition that describes this invocation
        /// </summary>
        CommandDefinition
    }
}