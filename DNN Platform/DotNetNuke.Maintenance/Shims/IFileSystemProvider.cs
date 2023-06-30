// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Common.Utilities;

    /// <summary>Abstraction on file system utilities to enable unit testing.</summary>
    internal interface IFileSystemProvider
    {
        /// <inheritdoc cref="FileStream(string, FileMode, FileAccess)"/>
        Stream CreateFileStream(string path, FileMode mode, FileAccess access);

        /// <inheritdoc cref="File.Delete(string)"/>
        void DeleteFile(string path);

        /// <inheritdoc cref="FileSystemUtils.DeleteFolderRecursive(string)"/>
        void DeleteFolderRecursive(string path);

        /// <inheritdoc cref="Directory.Exists(string)"/>
        bool DirectoryExists(string path);

        /// <inheritdoc cref="Directory.EnumerateDirectories(string, string, SearchOption)"/>
        IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

        /// <inheritdoc cref="File.Exists(string)"/>
        bool FileExists(string path);

        /// <inheritdoc cref="File.SetAttributes(string, FileAttributes)"/>
        void SetFileAttributes(string path, FileAttributes fileAttributes);
    }
}
