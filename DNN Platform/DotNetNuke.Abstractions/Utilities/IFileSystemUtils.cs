// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Utilities
{
    using System;

    /// <summary>
    /// File system utility.
    /// </summary>
    public interface IFileSystemUtils
    {
        /// <summary>
        /// Deletes a set of files.
        /// </summary>
        /// <param name="paths">
        /// An <see cref="Array"/> instance containing the file paths to delete.
        /// </param>
        /// <returns>
        /// A <see cref="string"/> containing all exception messages captured while deleting the files.
        /// </returns>
        string DeleteFiles(Array paths);

        /// <summary>
        /// Deletes a folders and all files and folders in it.
        /// </summary>
        /// <param name="path">The path of the folder to delete.</param>
        void DeleteFolderRecursive(string path);
    }
}
