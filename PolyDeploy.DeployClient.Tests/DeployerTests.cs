namespace PolyDeploy.DeployClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
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
            var deployer = new Deployer(renderer, packageFileSource, A.Fake<IInstaller>(), A.Fake<IEncryptor>());
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

            var deployer = new Deployer(renderer, packageFileSource, A.Fake<IInstaller>(), A.Fake<IEncryptor>());
            await deployer.StartAsync(A.Dummy<DeployInput>());

            actualFiles.ShouldBe(new[] { "Package 1.zip", "Another Package.zip" }, ignoreOrder: true);
        }

        [Fact]
        public async Task StartAsync_CallsGetSessionApi()
        {
            var installer = A.Fake<IInstaller>();

            var deployer = new Deployer(A.Fake<IRenderer>(), A.Fake<IPackageFileSource>(), installer, A.Fake<IEncryptor>());
            await deployer.StartAsync(A.Dummy<DeployInput>());

            A.CallTo(() => installer.StartSessionAsync(A<DeployInput>._)).MustHaveHappened();
        }

        [Fact]
        public async Task StartAsync_EncryptsPackages()
        {
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Package 1.zip", "Another Package.zip" });
            var package1Stream = new MemoryStream(Encoding.UTF8.GetBytes("This is a zip file"));
            A.CallTo(() => packageFileSource.GetFileStream("Package 1.zip")).Returns(package1Stream);
            var anotherPackageStream = new MemoryStream(Encoding.UTF8.GetBytes("This is another zip file"));
            A.CallTo(() => packageFileSource.GetFileStream("Another Package.zip")).Returns(anotherPackageStream);

            var encryptor = A.Fake<IEncryptor>();
            var encryptedPackage1Stream = new MemoryStream(Encoding.UTF8.GetBytes("This is an encrypted zip file"));
            var encryptedAnotherPackageStream = new MemoryStream(Encoding.UTF8.GetBytes("This is another encrypted zip file"));
            A.CallTo(() => encryptor.GetEncryptedStream(package1Stream)).Returns(encryptedPackage1Stream);
            A.CallTo(() => encryptor.GetEncryptedStream(anotherPackageStream)).Returns(encryptedAnotherPackageStream);

            var actualFiles = new Dictionary<string, string>();
            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.UploadPackageAsync(A<DeployInput>._, A<string>._, A<Stream>._, A<string>._))
                .Invokes((DeployInput DeployInput, string sessionId, Stream encryptedStream, string packageName) => actualFiles[packageName] = new StreamReader(encryptedStream).ReadToEnd());

            var deployer = new Deployer(A.Fake<IRenderer>(), packageFileSource, installer, encryptor);
            await deployer.StartAsync(A.Dummy<DeployInput>());

            actualFiles.ShouldBe(
                new Dictionary<string, string> {
                    {"Package 1.zip", "This is an encrypted zip file"},
                    {"Another Package.zip", "This is another encrypted zip file"},
                });
        }
    }
}
