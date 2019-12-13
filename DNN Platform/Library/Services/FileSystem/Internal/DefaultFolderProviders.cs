// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DotNetNuke.Services.FileSystem.Internal
{
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
