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
            var deployer = new Deployer(renderer, packageFileSource, A.Fake<IInstaller>());
            await deployer.StartAsync(A.Dummy<DeployInput>());

            A.CallTo(() => renderer.RenderListOfFiles(Array.Empty<string>())).MustHaveHappened();
        }

        [Fact]
        public async Task StartAsync_GivenPackageFiles_MustRenderFiles()
        {
            var actualFiles = new List<string>();
            var renderer = A.Fake<IRenderer>();
            A.CallTo(() => renderer.RenderListOfFiles(A<IEnumerable<string>>._))
             .Invokes((IEnumerable<string> files) => actualFiles.AddRange(files));
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Package 1.zip", "Another Package.zip" });

            var deployer = new Deployer(renderer, packageFileSource, A.Fake<IInstaller>());
            await deployer.StartAsync(A.Dummy<DeployInput>());

            actualFiles.ShouldBe(new[] { "Package 1.zip", "Another Package.zip" }, ignoreOrder: true);
        }

        [Fact]
        public async Task StartAsync_CallsGetSessionApi()
        {
            var installer = A.Fake<IInstaller>();

            var deployer = new Deployer(A.Fake<IRenderer>(), A.Fake<IPackageFileSource>(), installer);
            await deployer.StartAsync(A.Dummy<DeployInput>());

            A.CallTo(() => installer.StartSessionAsync(A<DeployInput>._)).MustHaveHappened();
        }
    }
}
