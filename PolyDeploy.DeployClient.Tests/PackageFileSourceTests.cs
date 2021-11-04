namespace PolyDeploy.DeployClient.Tests
{
    using System.IO.Abstractions;
    using System.IO;
    using System.Threading.Tasks;

    using FakeItEasy;

    using Shouldly;

    using Xunit;

    public class PackageFileSourceTests
    {
        [Fact]
        public void GetPackageFiles_GetsTheZipFilesInTheCurrentDirectory()
        {
            var fileSystem = A.Fake<IFileSystem>();

            A.CallTo(() => fileSystem.Directory.GetFiles(A<string>._, "*.zip")).Returns(new[] { "package1.zip", "package2.zip" });

            var fileSource = new PackageFileSource(fileSystem);
            var files = fileSource.GetPackageFiles();

            files.ShouldBe(new[] { "package1.zip", "package2.zip" }, ignoreOrder: true);
        }

        [Fact]
        public void GetFileStream_ReturnsStream()
        {
            var fileSystem = A.Fake<IFileSystem>();

            var stream = new MemoryStream();
            A.CallTo(() => fileSystem.File.Open("package1.zip", FileMode.Open)).Returns(stream);

            var fileSource = new PackageFileSource(fileSystem);
            var fileStream = fileSource.GetFileStream("package1.zip");

            fileStream.ShouldBe(stream);
        }
    }
}