// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    /// <summary>
    /// Represents different types of Content Security Policy source types.
    /// </summary>
    public enum CspSourceType
    {
        /// <summary>
        /// Allows specifying specific domains as sources.
        /// </summary>
        Host,

        /// <summary>
        /// Allows specifying protocols (e.g., https:, data:) as sources.
        /// </summary>
        Scheme,

        /// <summary>
        /// Allows resources from the same origin ('self').
        /// </summary>
        Self,

        /// <summary>
        /// Allows the use of inline code ('unsafe-inline').
        /// </summary>
        Inline,

        /// <summary>
        /// Allows the use of eval() ('unsafe-eval').
        /// </summary>
        Eval,

        /// <summary>
        /// Uses a cryptographic nonce to validate resources.
        /// </summary>
        Nonce,

        /// <summary>
        /// Uses a cryptographic hash to validate resources.
        /// </summary>
        Hash,

        /// <summary>
        /// Allows no sources ('none').
        /// </summary>
        None,

        /// <summary>
        /// Enables strict-dynamic mode for script loading.
        /// </summary>
        StrictDynamic,
    }
}
