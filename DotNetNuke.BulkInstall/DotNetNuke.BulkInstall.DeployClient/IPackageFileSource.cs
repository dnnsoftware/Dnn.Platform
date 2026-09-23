// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System.Collections.Generic;
using System.IO;

/// <summary>A contract specifying the ability to read the file system.</summary>
public interface IPackageFileSource
{
    /// <summary>Gets a listing of files at the given <paramref name="path"/>.</summary>
    /// <param name="path">The path to the root folder in which to look for packages.</param>
    /// <param name="searchOption">Whether to search only the directory at <paramref name="path"/> or also subdirectories.</param>
    /// <returns>A collection of paths.</returns>
    IReadOnlyCollection<string> GetPackageFiles(string path, SearchOption searchOption);

    /// <summary>Gets the contents of a file.</summary>
    /// <param name="fileName">The path to the file.</param>
    /// <returns>A <see cref="Stream"/> with the file contents.</returns>
    Stream GetFileStream(string fileName);
}
