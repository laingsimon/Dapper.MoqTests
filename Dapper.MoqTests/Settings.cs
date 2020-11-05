﻿using System;
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

        public ISqlParametersBuilder SqlParametersBuilder { get; set; } = new ParametersObjectBuilder();
        public IEqualityComparer<string> SqlTextComparer { get; set; } = new SqlTextComparer();
        public IParametersComparer SqlParametersComparer { get; set; } = new ParametersComparer(StringComparer.OrdinalIgnoreCase);

        public IIdentityComparer IdentityComparer = new DapperIdentityComparer();

        public Unresolved Unresolved { get; } = new Unresolved();

        public static Settings Default = new Settings();
    }
}
