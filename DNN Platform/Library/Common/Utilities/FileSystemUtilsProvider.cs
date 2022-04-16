// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;

    using DotNetNuke.Abstractions.Utilities;

    /// <inheritdoc />
    public class FileSystemUtilsProvider : IFileSystemUtils
    {
        /// <inheritdoc/>
        public void DeleteFolderRecursive(string path)
        {
            FileSystemUtils.DeleteFolderRecursive(path);
        }

        /// <inheritdoc/>
        public string DeleteFiles(Array paths)
        {
            return FileSystemUtils.DeleteFiles(paths);
        }
    }
}
