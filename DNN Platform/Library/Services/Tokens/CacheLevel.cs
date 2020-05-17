﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Services.Tokens
{
    /// <summary>
    /// CacheLevel is used to specify the cachability of a string, determined as minimum of the used token cachability
    /// </summary>
    /// <remarks>
    /// CacheLevel is determined as minimum of the used tokens' cachability 
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
        fullyCacheable = 10
    }
}
