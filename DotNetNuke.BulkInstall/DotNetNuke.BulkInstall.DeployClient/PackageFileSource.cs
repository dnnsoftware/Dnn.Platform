// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

/// <summary>The <see cref="IPackageFileSource"/> implementation, using <see cref="IFileSystem"/>.</summary>
public class PackageFileSource : IPackageFileSource
{
    private readonly IFileSystem fileSystem;

    /// <summary>Initializes a new instance of the <see cref="PackageFileSource"/> class.</summary>
    /// <param name="fileSystem">The file system.</param>
    public PackageFileSource(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<string> GetPackageFiles(string path)
    {
        return this.fileSystem.Directory.GetFiles(path, "*.zip");
    }

    /// <inheritdoc/>
    public Stream GetFileStream(string fileName)
    {
        return this.fileSystem.File.Open(fileName, FileMode.Open, FileAccess.Read);
    }
}
