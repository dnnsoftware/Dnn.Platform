// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    /// <summary>ModuleCachingType is an enum that provides the caching types for Module Content.</summary>
    public enum ModuleCachingType
    {
        /// <summary>Caching in memory.</summary>
        Memory = 0,

        /// <summary>Caching to files on disk.</summary>
        Disk = 1,

        /// <summary>Caching in the database.</summary>
        Database = 2,
    }
}
