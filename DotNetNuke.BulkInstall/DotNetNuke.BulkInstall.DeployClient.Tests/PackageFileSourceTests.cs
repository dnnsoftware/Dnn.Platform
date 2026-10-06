// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.BulkInstall.DeployClient;

using System.IO.Abstractions;

using DotNetNuke.BulkInstall.DeployClient;

public class PackageFileSourceTests
{
    [InlineData("")]
    [InlineData("path/to/packages")]
    [Theory]
    public void GetPackageFiles_GetsTheZipFilesInTheGivenDirectory(string path)
    {
        var fileSystem = A.Fake<IFileSystem>();

        A.CallTo(() => fileSystem.Directory.GetFiles(path, "*.zip", SearchOption.TopDirectoryOnly)).Returns(new[] { "package1.zip", "package2.zip" });

        var fileSource = new PackageFileSource(fileSystem);
        var files = fileSource.GetPackageFiles(path, SearchOption.TopDirectoryOnly);

        files.ShouldBe(new[] { "package1.zip", "package2.zip" }, ignoreOrder: true);
    }

    [InlineData("")]
    [InlineData("path/to/packages")]
    [Theory]
    public void GetPackageFiles_GetsTheZipFilesInTheGivenDirectoryRecursively(string path)
    {
        var fileSystem = A.Fake<IFileSystem>();

        var expected = new[] { Path.Combine(path, "a", "package1.zip"), Path.Combine(path, "b", "package2.zip"), };
        A.CallTo(() => fileSystem.Directory.GetFiles(path, "*.zip", SearchOption.AllDirectories)).Returns(expected);

        var fileSource = new PackageFileSource(fileSystem);
        var files = fileSource.GetPackageFiles(path, SearchOption.AllDirectories);

        files.ShouldBe(expected, ignoreOrder: true);
    }

    [Fact]
    public void GetFileStream_ReturnsStream()
    {
        var fileSystem = A.Fake<IFileSystem>();

        var stream = new FakeStream();
        A.CallTo(() => fileSystem.File.Open("package1.zip", FileMode.Open, FileAccess.Read)).Returns(stream);

        var fileSource = new PackageFileSource(fileSystem);
        var fileStream = fileSource.GetFileStream("package1.zip");

        fileStream.ShouldBe(stream);
    }

    private class FakeStream : FileSystemStream
    {
        public FakeStream(Stream? stream = null, string? path = null, bool isAsync = false)
            : base(stream ?? new MemoryStream(), path ?? ".", isAsync)
        {
        }
    }
}
