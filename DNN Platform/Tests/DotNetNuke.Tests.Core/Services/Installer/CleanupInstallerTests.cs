// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Installer
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Utilities;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Installers;
    using DotNetNuke.Services.Installer.Packages;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CleanupInstallerTests
    {
        private const string wwwroot = @"D:\inetpub\wwwroot";

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(@"C:\")]
        [TestCase(wwwroot)]
        [TestCase(@"\")]
        [TestCase(@"\\")]
        [TestCase(@"\\fileshare")]
        [TestCase(@"C:/")]
        [TestCase(@"D:/inetpub/wwwroot")]
        [TestCase(@"/")]
        [TestCase(@"//")]
        [TestCase(@"//fileshare")]
        [TestCase(@"file://C:/Windows")]
        [TestCase(@"http://dnndev.me")]
        public void Install_WhenFolderInvalid_DoesNotCallFileSystem(string path)
        {
            // arrange
            var applicationStatusInfoMock = SetupApplicationStatusInfoMock();

            var fileSystemUtilsMock = new Mock<IFileSystemUtils>();

            var sut = new CleanupInstaller(applicationStatusInfoMock.Object, fileSystemUtilsMock.Object)
            {
                Package = new PackageInfo(new InstallerInfo()),
            };

            // act
            sut.ProcessFolder(path);
            sut.Install();

            // assert
            Assert.IsTrue(sut.Completed);
            fileSystemUtilsMock.Verify(x => x.DeleteFolderRecursive(It.IsAny<string>()), Times.Never);
        }

        [Test]
        [TestCase(wwwroot + @"\dir")]
        [TestCase(wwwroot + @"/dir")]
        public void Install_WhenFolderValid_CallsFileSystem(string path)
        {
            // arrange
            var applicationStatusInfoMock = SetupApplicationStatusInfoMock();

            var fileSystemUtilsMock = new Mock<IFileSystemUtils>();

            fileSystemUtilsMock
                .Setup(x => x.DeleteFolderRecursive(It.IsAny<string>()))
                .Verifiable();

            var sut = new CleanupInstaller(applicationStatusInfoMock.Object, fileSystemUtilsMock.Object)
            {
                Package = new PackageInfo(new InstallerInfo()),
            };

            // act
            sut.ProcessFolder(path);
            sut.Install();

            // assert
            Assert.IsTrue(sut.Completed);
            fileSystemUtilsMock.Verify();
        }

        private static Mock<IApplicationStatusInfo> SetupApplicationStatusInfoMock()
        {
            var mock = new Mock<IApplicationStatusInfo>();

            mock.SetupGet(x => x.ApplicationMapPath)
                .Returns(wwwroot);

            return mock;
        }
    }
}
