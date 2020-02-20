// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Framework.JavaScriptLibraries
{
    /// <summary>
    /// determine which version of a script is to be used
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
