// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// This class contains a method to return the list of names of the default folder providers.
    /// </summary>
    /// <remarks>
    /// This class is reserved for internal use and is not intended to be used directly from your code.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DefaultFolderProviders
    {
        /// <summary>
        /// Returns a list with the names of the default folder providers.
        /// </summary>
        /// <returns>The list of names of the default folder providers.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static List<string> GetDefaultProviders()
        {
            return new List<string> { "StandardFolderProvider", "SecureFolderProvider", "DatabaseFolderProvider" };
        }
    }
}
