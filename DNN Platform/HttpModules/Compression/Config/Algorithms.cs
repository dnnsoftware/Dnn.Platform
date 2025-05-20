// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.Compression;

/// <summary>The available compression algorithms to use with the HttpCompressionModule.</summary>
public enum Algorithms
{
    /// <summary>Compress using the Deflate algorithm.</summary>
    Deflate = 2,

    /// <summary>Compress using the GZip algorithm.</summary>
    GZip = 1,

    /// <summary>Do not compress.</summary>
    None = 0,

    /// <summary>Compress using the default algorithm.</summary>
    Default = -1,
}
