// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Shims
{
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Common;

    /// <summary>Abstraction on file system utilities to enable unit testing.</summary>
    internal sealed class FileSystemProviderShim : IFileSystemProvider
    {
        /// <inheritdoc />
        public Stream CreateFileStream(string path, FileMode mode, FileAccess access)
        {
            return new FileStream(path, mode, access);
        }

        /// <inheritdoc />
        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        /// <inheritdoc />
        public void DeleteFolderRecursive(string path)
        {
            Globals.DeleteFolderRecursive(path);
        }

        /// <inheritdoc />
        public bool DirectoryExists(string path) => Directory.Exists(path);

        /// <inheritdoc />
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) =>
            Directory.EnumerateFiles(path, searchPattern, searchOption);

        /// <inheritdoc />
        public bool FileExists(string path) => File.Exists(path);

        /// <inheritdoc />
        public void SetFileAttributes(string path, FileAttributes fileAttributes)
        {
            File.SetAttributes(path, fileAttributes);
        }
    }
}
