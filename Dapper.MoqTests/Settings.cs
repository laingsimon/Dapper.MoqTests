namespace Dapper.MoqTests
{
    public static class Settings
    {
        /// <summary>
        /// Reset the cache of Dapper commands after each command has executed, thus permitting identification
        /// of different return types for the same text, parameters and command-type
        /// </summary>
        public static bool ResetDapperCachePerCommand { get; set; }
    }
}
