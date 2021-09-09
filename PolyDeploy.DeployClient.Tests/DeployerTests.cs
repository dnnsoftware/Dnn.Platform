namespace PolyDeploy.DeployClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using FakeItEasy;

    using Shouldly;

    using Xunit;

    public class DeployerTests
    {
        [Fact]
        public async Task StartAsync_GivenRenderer_MustRenderFiles()
        {
            var renderer = A.Fake<IRenderer>();
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(Array.Empty<string>());
            var deployer = new Deployer(renderer, packageFileSource);
            await deployer.StartAsync();

            A.CallTo(() => renderer.RenderListOfFiles(Array.Empty<string>())).MustHaveHappened();
        }

        [Fact]
        public async Task StartAsync_GivenPackageFiles_MustRenderFiles()
        {
            var actualFiles = new List<string>();
            var renderer = A.Fake<IRenderer>();
            A.CallTo(() => renderer.RenderListOfFiles(A<IEnumerable<string>>._))
             .ReturnsLazily(async (IEnumerable<string> files) => actualFiles.AddRange(files));
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Package 1.zip", "Another Package.zip" });

            var deployer = new Deployer(renderer, packageFileSource);
            await deployer.StartAsync();

            actualFiles.ShouldBe(new[] { "Package 1.zip", "Another Package.zip" }, ignoreOrder: true);
        }
    }

    public interface IPackageFileSource
    {
        Task<IReadOnlyCollection<string>> GetPackageFiles();
    }

    public interface IDeployer
    {
        Task StartAsync();
    }

    public class Deployer : IDeployer
    {

        private readonly IRenderer renderer;
        private readonly IPackageFileSource packageFileSource;

        public Deployer(IRenderer renderer, IPackageFileSource packageFileSource)
        {
            this.renderer = renderer;
            this.packageFileSource = packageFileSource;
        }

        public async Task StartAsync()
        {
            var packageFiles = await this.packageFileSource.GetPackageFiles();
            await this.renderer.RenderListOfFiles(packageFiles);
        }
    }

    public interface IRenderer
    {
        Task RenderListOfFiles(IEnumerable<string> files);
    }
}