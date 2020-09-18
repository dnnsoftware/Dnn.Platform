// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.Compression
{
    /// <summary>
    /// The available compression algorithms to use with the HttpCompressionModule.
    /// </summary>
    public enum Algorithms
    {
        Deflate = 2,
        GZip = 1,
        None = 0,
        Default = -1,
    }
}
