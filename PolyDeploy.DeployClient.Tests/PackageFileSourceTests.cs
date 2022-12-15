namespace PolyDeploy.DeployClient.Tests
{
    using System.IO.Abstractions;

    public class PackageFileSourceTests
    {
        [InlineData("")]
        [InlineData("path/to/packages")]
        [Theory]
        public void GetPackageFiles_GetsTheZipFilesInTheGivenDirectory(string path)
        {
            var fileSystem = A.Fake<IFileSystem>();

            A.CallTo(() => fileSystem.Directory.GetFiles(path, "*.zip")).Returns(new[] { "package1.zip", "package2.zip" });

            var fileSource = new PackageFileSource(fileSystem);
            var files = fileSource.GetPackageFiles(path);

            files.ShouldBe(new[] { "package1.zip", "package2.zip" }, ignoreOrder: true);
        }
        
        [Fact]
        public void GetFileStream_ReturnsStream()
        {
            var fileSystem = A.Fake<IFileSystem>();

            var stream = new MemoryStream();
            A.CallTo(() => fileSystem.File.Open("package1.zip", FileMode.Open, FileAccess.Read)).Returns(stream);

            var fileSource = new PackageFileSource(fileSystem);
            var fileStream = fileSource.GetFileStream("package1.zip");

            fileStream.ShouldBe(stream);
        }
    }
}
