using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Dapper.MoqTests
{
    public class Settings
    {
        /// <summary>
        /// Reset the cache of Dapper commands after each command has executed, thus permitting identification
        /// of different return types for the same text, parameters and command-type
        /// </summary>
        public bool ResetDapperCachePerCommand { get; set; }

        /// <summary>
        /// The type that should be used to build the parameters object type
        /// </summary>
        public ISqlParametersBuilder SqlParametersBuilder { get; set; } = new ParametersObjectBuilder();

        /// <summary>
        /// The sql text comparer
        /// </summary>
        public IEqualityComparer<string> SqlTextComparer { get; set; } = new SqlTextComparer();

        /// <summary>
        /// The parameter comparer type
        /// </summary>
        public IParametersComparer SqlParametersComparer { get; set; } = new ParametersComparer(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The dapper identity comparer
        /// </summary>
        public IIdentityComparer IdentityComparer = new DapperIdentityComparer(new DapperCommandTextHelper());

        /// <summary>
        /// Settings for where details haven't been resolved
        /// </summary>
        public Unresolved Unresolved { get; } = new Unresolved();

        /// <summary>
        /// Where Dapper method calls are ambiguous between CommandDefinition vs other implementations, prefer CommandDefinition overloads
        /// </summary>
        public bool PreferCommandDefinitions { get; set; }

        public IDapperCommandTextHelper CommandTextHelper { get; set; } = new DapperCommandTextHelper();

        /// <summary>
        /// The default set of settings
        /// </summary>
        public static Settings Default = new Settings();
    }
}
