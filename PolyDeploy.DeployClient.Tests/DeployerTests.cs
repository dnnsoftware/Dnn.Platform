namespace PolyDeploy.DeployClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using FakeItEasy;

    using Shouldly;

    using Xunit;
    using Xunit.Abstractions;

    public class DeployerTests
    {
        [Fact]
        public async Task StartAsync_WelcomesUsers()
        {
            var renderer = A.Fake<IRenderer>();
            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.GetSessionAsync(A<DeployInput>._, A<string>._)).Returns(new Session { Status = SessionStatus.Complete, });
            var deployer = new Deployer(renderer, A.Fake<IPackageFileSource>(), installer, A.Fake<IEncryptor>(), A.Fake<IDelayer>());
            await deployer.StartAsync(A.Dummy<DeployInput>());

            A.CallTo(() => renderer.Welcome()).MustHaveHappened();
        }

        [Fact]
        public async Task StartAsync_GivenRenderer_MustRenderFiles()
        {
            var renderer = A.Fake<IRenderer>();
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(Array.Empty<string>());
            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.GetSessionAsync(A<DeployInput>._, A<string>._)).Returns(new Session { Status = SessionStatus.Complete, });

            var deployer = new Deployer(renderer, packageFileSource, installer, A.Fake<IEncryptor>(), A.Fake<IDelayer>());
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
            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.GetSessionAsync(A<DeployInput>._, A<string>._)).Returns(new Session { Status = SessionStatus.Complete, });

            var deployer = new Deployer(renderer, packageFileSource, installer, A.Fake<IEncryptor>(), A.Fake<IDelayer>());
            await deployer.StartAsync(A.Dummy<DeployInput>());

            actualFiles.ShouldBe(new[] { "Package 1.zip", "Another Package.zip" }, ignoreOrder: true);
        }

        [Fact]
        public async Task StartAsync_CallsGetSessionApi()
        {
            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.GetSessionAsync(A<DeployInput>._, A<string>._)).Returns(new Session { Status = SessionStatus.Complete, });

            var deployer = new Deployer(A.Fake<IRenderer>(), A.Fake<IPackageFileSource>(), installer, A.Fake<IEncryptor>(), A.Fake<IDelayer>());
            await deployer.StartAsync(A.Dummy<DeployInput>());

            A.CallTo(() => installer.StartSessionAsync(A<DeployInput>._)).MustHaveHappened();
        }

        [Fact]
        public async Task StartAsync_EncryptsPackages()
        {
            var options = A.Dummy<DeployInput>();
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Package 1.zip", "Another Package.zip" });
            var package1Stream = new MemoryStream(Encoding.UTF8.GetBytes("This is a zip file"));
            A.CallTo(() => packageFileSource.GetFileStream("Package 1.zip")).Returns(package1Stream);
            var anotherPackageStream = new MemoryStream(Encoding.UTF8.GetBytes("This is another zip file"));
            A.CallTo(() => packageFileSource.GetFileStream("Another Package.zip")).Returns(anotherPackageStream);

            var encryptor = A.Fake<IEncryptor>();
            var encryptedPackage1Stream = new MemoryStream(Encoding.UTF8.GetBytes("This is an encrypted zip file"));
            var encryptedAnotherPackageStream = new MemoryStream(Encoding.UTF8.GetBytes("This is another encrypted zip file"));
            A.CallTo(() => encryptor.GetEncryptedStream(options, package1Stream)).Returns(encryptedPackage1Stream);
            A.CallTo(() => encryptor.GetEncryptedStream(options, anotherPackageStream)).Returns(encryptedAnotherPackageStream);

            var sessionId = Guid.NewGuid().ToString();
            var actualFiles = new Dictionary<string, string>();
            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.StartSessionAsync(options)).Returns(sessionId);
            A.CallTo(() => installer.UploadPackageAsync(A<DeployInput>._, sessionId, A<Stream>._, A<string>._))
                .Invokes((DeployInput DeployInput, string sessionId, Stream encryptedStream, string packageName) => actualFiles[packageName] = new StreamReader(encryptedStream).ReadToEnd());
            A.CallTo(() => installer.GetSessionAsync(options, sessionId)).Returns(new Session { Status = SessionStatus.Complete, });

            var deployer = new Deployer(new FakeRenderer(), packageFileSource, installer, encryptor, A.Fake<IDelayer>());
            await deployer.StartAsync(options);

            actualFiles.ShouldBe(
                new Dictionary<string, string> {
                    {"Package 1.zip", "This is an encrypted zip file"},
                    {"Another Package.zip", "This is another encrypted zip file"},
                });
        }

        [Fact]
        public async Task StartAsync_RendersFileUploadStatus()
        {
            IEnumerable<(string, Task)>? uploads = null;
            var options = A.Dummy<DeployInput>();
            var renderer = A.Fake<IRenderer>();
            A.CallTo(() => renderer.RenderFileUploadsAsync(A<IEnumerable<(string, Task)>>._))
             .Invokes((IEnumerable<(string, Task)> theUploads) => uploads = theUploads); ;
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Install.zip", });
            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.GetSessionAsync(options, A<string>._)).Returns(new Session { Status = SessionStatus.Complete, });

            var deployer = new Deployer(renderer, packageFileSource, installer, A.Fake<IEncryptor>(), A.Fake<IDelayer>());
            await deployer.StartAsync(options);

            uploads.ShouldNotBeNull();
            var (file, task) = uploads.ShouldHaveSingleItem();
            file.ShouldBe("Install.zip");
        }

        [Fact]
        public async Task StartAsync_StartsInstallation()
        {
            var sessionId = Guid.NewGuid().ToString();
            var options = A.Dummy<DeployInput>();
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Package 1.zip", });

            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.StartSessionAsync(options)).Returns(sessionId);
            A.CallTo(() => installer.GetSessionAsync(options, sessionId)).Returns(new Session { Status = SessionStatus.Complete, });

            var deployer = new Deployer(new FakeRenderer(), packageFileSource, installer, A.Fake<IEncryptor>(), A.Fake<IDelayer>());
            await deployer.StartAsync(options);

            A.CallTo(() => installer.UploadPackageAsync(options, A<string>._, A<Stream>._, A<string>._)).MustHaveHappened()
             .Then(A.CallTo(() => installer.InstallPackagesAsync(options, sessionId)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task StartAsync_DoesNotWaitForInstallationResponse()
        {
            var sessionId = Guid.NewGuid().ToString();
            var options = A.Dummy<DeployInput>();
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Package 1.zip", });

            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.StartSessionAsync(options)).Returns(sessionId);
            A.CallTo(() => installer.InstallPackagesAsync(options, sessionId)).Returns(
                Task.FromException(new InvalidOperationException("This method does not return until the installation is complete and should not be awaited")));
            A.CallTo(() => installer.GetSessionAsync(options, sessionId)).Returns(new Session { Status = SessionStatus.Complete, });

            var deployer = new Deployer(new FakeRenderer(), packageFileSource, installer, A.Fake<IEncryptor>(), A.Fake<IDelayer>());
            await Should.NotThrowAsync(async () => await deployer.StartAsync(options));
        }

        [Fact]
        public async Task StartAsync_CallsGetSessionUntilInstallationIsComplete()
        {
            var sessionId = Guid.NewGuid().ToString();
            var options = A.Dummy<DeployInput>();
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Package 1.zip", "Package 2.zip" });

            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.StartSessionAsync(options))
             .Returns(sessionId);
            A.CallTo(() => installer.GetSessionAsync(options, sessionId))
             .ReturnsNextFromSequence(
                new Session { Status = SessionStatus.NotStarted, Responses = null, },
                new Session { Status = SessionStatus.InProgess, Responses = null, },
                new Session
                {
                    Status = SessionStatus.InProgess,
                    Responses = new SortedList<int, SessionResponse?>
                    {
                        { 0, new SessionResponse { Name = "Package 1.zip", Attempted = true, CanInstall = true, Success = true, Failures = null, Packages = new List<PackageResponse?> { new PackageResponse { CanInstall = true, Dependencies = new List<DependencyResponse?>(0), Name = "Package 1", VersionStr = "1.10.1", }, } } },
                        { 1, new SessionResponse { Name = "Package 2.zip", Attempted = false, CanInstall = true, Success = false, Failures = null, Packages = new List<PackageResponse?> { new PackageResponse { CanInstall = true, Dependencies = new List<DependencyResponse?>(0), Name = "Package 2", VersionStr = "2.20.2", }, } } },
                    },
                },
                new Session
                {
                    Status = SessionStatus.Complete,
                    Responses = new SortedList<int, SessionResponse?>
                    {
                        { 0, new SessionResponse { Name = "Package 1.zip", Attempted = true, CanInstall = true, Success = true, Failures = null, Packages = new List<PackageResponse?> { new PackageResponse { CanInstall = true, Dependencies = new List<DependencyResponse?>(0), Name = "Package 1", VersionStr = "1.10.1", }, } } },
                        { 1, new SessionResponse { Name = "Package 2.zip", Attempted = true, CanInstall = true, Success = true, Failures = null, Packages = new List<PackageResponse?> { new PackageResponse { CanInstall = true, Dependencies = new List<DependencyResponse?>(0), Name = "Package 2", VersionStr = "2.20.2", }, } } },
                    },
                }
            );

            var delayer = A.Fake<IDelayer>();
            A.CallTo(() => delayer.Delay(TimeSpan.FromSeconds(1))).Returns(Task.CompletedTask).NumberOfTimes(3);

            var deployer = new Deployer(new FakeRenderer(), packageFileSource, installer, A.Fake<IEncryptor>(), delayer);
            await deployer.StartAsync(options);

            A.CallTo(() => installer.GetSessionAsync(options, sessionId)).MustHaveHappened(4, Times.Exactly);
        }

        [Fact]
        public async Task StartAsync_RendersSessionOverview()
        {
            var sessionId = Guid.NewGuid().ToString();
            var options = A.Dummy<DeployInput>();
            var packageFileSource = A.Fake<IPackageFileSource>();
            A.CallTo(() => packageFileSource.GetPackageFiles()).Returns(new[] { "Package 1.zip", "Package 2.zip" });

            var installer = A.Fake<IInstaller>();
            A.CallTo(() => installer.StartSessionAsync(options))
             .Returns(sessionId);
            A.CallTo(() => installer.GetSessionAsync(options, sessionId))
             .ReturnsNextFromSequence(
                new Session { Status = SessionStatus.NotStarted, Responses = null, },
                new Session { Status = SessionStatus.InProgess, Responses = null, },
                new Session
                {
                    Status = SessionStatus.InProgess,
                    Responses = new SortedList<int, SessionResponse?>
                    {
                        { 0, new SessionResponse { Name = "Package 1.zip", Attempted = true, CanInstall = true, Success = true, Failures = null, Packages = new List<PackageResponse?> { new PackageResponse { CanInstall = true, Dependencies = new List<DependencyResponse?>(0), Name = "Package 1", VersionStr = "1.10.1", }, } } },
                        { 1, new SessionResponse { Name = "Package 2.zip", Attempted = false, CanInstall = true, Success = false, Failures = null, Packages = new List<PackageResponse?> { new PackageResponse { CanInstall = true, Dependencies = new List<DependencyResponse?>(0), Name = "Package 2", VersionStr = "2.20.2", }, } } },
                    },
                },
                new Session
                {
                    Status = SessionStatus.Complete,
                    Responses = new SortedList<int, SessionResponse?>
                    {
                        { 0, new SessionResponse { Name = "Package 1.zip", Attempted = true, CanInstall = true, Success = true, Failures = null, Packages = new List<PackageResponse?> { new PackageResponse { CanInstall = true, Dependencies = new List<DependencyResponse?>(0), Name = "Package 1", VersionStr = "1.10.1", }, } } },
                        { 1, new SessionResponse { Name = "Package 2.zip", Attempted = true, CanInstall = true, Success = true, Failures = null, Packages = new List<PackageResponse?> { new PackageResponse { CanInstall = true, Dependencies = new List<DependencyResponse?>(0), Name = "Package 2", VersionStr = "2.20.2", }, } } },
                    },
                }
            );

            var fakeRenderer = new FakeRenderer();
            var deployer = new Deployer(fakeRenderer, packageFileSource, installer, A.Fake<IEncryptor>(), A.Fake<IDelayer>());
            await deployer.StartAsync(options);

            var overview = fakeRenderer.InstallationOverview.ShouldNotBeNull();
            overview.Count.ShouldBe(2);
            var package1 = overview[0].ShouldNotBeNull();
            package1.Name.ShouldBe("Package 1.zip");
            var package2 = overview[1].ShouldNotBeNull();
            package2.Attempted.ShouldBeFalse();
        }

        private class FakeRenderer : IRenderer
        {
            public SortedList<int, SessionResponse?>? InstallationOverview { get; private set; }

            public async Task RenderFileUploadsAsync(IEnumerable<(string file, Task uploadTask)> uploads)
            {
                await Task.WhenAll(uploads.Select(u => u.uploadTask));
            }

            public void RenderInstallationOverview(SortedList<int, SessionResponse?> packageFiles)
            {
                this.InstallationOverview = packageFiles;
            }

            public void Welcome()
            {
            }

            public void RenderListOfFiles(IEnumerable<string> files)
            {
            }
        }
    }
}
