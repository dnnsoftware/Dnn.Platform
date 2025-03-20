// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Utility class for converting source types to their string representations.
    /// </summary>
    public static class CspSourceTypeNameMapper
    {
        /// <summary>
        /// Gets the source type name string.
        /// </summary>
        /// <param name="sourceType">The source type to get the name for.</param>
        /// <returns>The source type name string.</returns>
        public static string GetSourceTypeName(CspSourceType sourceType)
        {
            return sourceType switch
            {
                CspSourceType.Host => "host",
                CspSourceType.Scheme => "scheme",
                CspSourceType.Self => "'self'",
                CspSourceType.Inline => "'unsafe-inline'",
                CspSourceType.Eval => "'unsafe-eval'",
                CspSourceType.Nonce => "nonce",
                CspSourceType.Hash => "hash",
                CspSourceType.None => "none",
                CspSourceType.StrictDynamic => "strict-dynamic",
                _ => throw new ArgumentException("Unknown source type")
            };
        }
    }
}
