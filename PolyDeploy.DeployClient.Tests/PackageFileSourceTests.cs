namespace PolyDeploy.DeployClient.Tests
{
    using System.Threading.Tasks;

    using Xunit;

    public class PackageFileSourceTests
    {
        [Fact]
        public async Task GetPackageFiles_GetsTheZipFilesInTheCurrentDirectory()
        {
            var fileSource = new PackageFileSource();
            await fileSource.GetPackageFiles();
        }
    }
}