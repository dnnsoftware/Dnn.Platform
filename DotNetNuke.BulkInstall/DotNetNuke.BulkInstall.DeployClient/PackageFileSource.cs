// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.BulkInstall.DeployClient
{
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using System.IO;

    public class PackageFileSource : IPackageFileSource
    {
        private readonly IFileSystem fileSystem;

        public PackageFileSource(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public IReadOnlyCollection<string> GetPackageFiles(string path)
        {
            return this.fileSystem.Directory.GetFiles(path, "*.zip");
        }

        public Stream GetFileStream(string fileName)
        {
            return this.fileSystem.File.Open(fileName, FileMode.Open, FileAccess.Read);
        }
    }
}
