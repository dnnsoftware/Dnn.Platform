namespace PolyDeploy.DeployClient.Tests
{
    using System.IO.Abstractions;
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

            files.Count.ShouldBe(2);
            files.ShouldContain("package1.zip");
            files.ShouldContain("package2.zip");
        }
    }
}