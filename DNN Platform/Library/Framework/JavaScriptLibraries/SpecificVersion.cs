// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    /// <summary>
    /// determine which version of a script is to be used.
    /// </summary>
    public enum SpecificVersion
    {
        /// <summary>The most recent version</summary>
        Latest,

        /// <summary>Match the major version</summary>
        LatestMajor,

        /// <summary>Match the major and minor versions</summary>
        LatestMinor,

        /// <summary>Match version exactly</summary>
        Exact,
    }
}
