// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library
{
    /// <summary>
    ///
    /// </summary>
    public enum ServiceScope
    {
        /// <summary>
        /// the service available for all users.
        /// </summary>
        Regular,

        /// <summary>
        /// the service only available for admin users.
        /// </summary>
        Admin,

        /// <summary>
        /// the service only available for host users.
        /// </summary>
        Host,
    }
}
