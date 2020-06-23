// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// CacheLevel is used to specify the cachability of a string, determined as minimum of the used token cachability.
    /// </summary>
    /// <remarks>
    /// CacheLevel is determined as minimum of the used tokens' cachability.
    /// </remarks>
    public enum CacheLevel : byte
    {
        /// <summary>
        /// Caching of the text is not suitable and might expose security risks
        /// </summary>
        notCacheable = 0,

        /// <summary>
        /// Caching of the text might result in inaccurate display (e.g. time), but does not expose a security risk
        /// </summary>
        secureforCaching = 5,

        /// <summary>
        /// Caching of the text can be done without limitations or any risk
        /// </summary>
        fullyCacheable = 10,
    }
}
