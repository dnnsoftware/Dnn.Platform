// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.HttpModules.Compression
{
    /// <summary>
    /// The available compression algorithms to use with the HttpCompressionModule
    /// </summary>
    public enum Algorithms
    {
        Deflate = 2,
        GZip = 1,
        None = 0,
        Default = -1
    }
}
