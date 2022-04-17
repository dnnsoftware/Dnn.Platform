// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Installer
{
    using System.Collections.Generic;

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
        private const string wwwroot = "D:/inetpub/wwwroot";

        [Test]
        [TestCaseSource(nameof(InvalidPathsTestCaseSource))]
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
        [TestCaseSource(nameof(ValidPathsTestCaseSource))]
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

        private static IEnumerable<string> InvalidPathsTestCaseSource()
        {
            yield return null;
            yield return string.Empty;
            yield return " ";
            yield return ".";
            yield return "..";
            yield return "...";

            var relativeSlashedPaths = new[]
            {
                "../",
                "../wwwroot",
                "../wwwroot/",
                "../wwwroot/..",
                "../wwwroot/../",
                "../wwwroot/dir",
                "Providers/../..",
                "Providers/../../",
                "dir/../dir",
                "dir/../dir/",
                "dir/../../wwwroot/dir",
                "dir/../../wwwroot/dir/",
            };

            foreach (var path in relativeSlashedPaths)
            {
                yield return path;
                yield return Alternate(path);
                yield return MakeAbsolute(path);
                yield return Alternate(MakeAbsolute(path));
            }

            var absoluteSlashedPaths = new[]
            {
                "C:/",
                wwwroot,
                wwwroot + "../wwwroot/dir",
                "/",
                "//",
                "///",
                "//fileshare",
                "file://C:/Windows",
                "http://dnndev.me",
            };

            foreach (var path in absoluteSlashedPaths)
            {
                yield return path;
                yield return Alternate(path);
            }
        }

        private static IEnumerable<string> ValidPathsTestCaseSource()
        {
            yield return "dir";

            var relativeSlashedPaths = new[]
            {
                "./dir",
            };

            foreach (var path in relativeSlashedPaths)
            {
                yield return path;
                yield return Alternate(path);
            }
        }

        private static string MakeAbsolute(string relative)
        {
            return string.Concat(wwwroot, "/", relative);
        }

        private static string Alternate(string path)
        {
            return path.Replace('/', '\\');
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
