// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Installer
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Internal;
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
            fileSystemUtilsMock.Verify(x => x.DeleteEmptyFoldersRecursive(It.IsAny<string>()), Times.Never);
        }

        [Test]
        [TestCaseSource(nameof(ValidPathsTestCaseSource))]
        public void Install_WhenFolderValid_CallsFileSystem(Tuple<string, string> paths)
        {
            // arrange
            var actualPath = default(string);

            var applicationStatusInfoMock = SetupApplicationStatusInfoMock();

            var fileSystemUtilsMock = new Mock<IFileSystemUtils>();

            fileSystemUtilsMock
                .Setup(x => x.DeleteEmptyFoldersRecursive(It.IsAny<string>()))
                .Callback((string fullPath) =>
                {
                    actualPath = fullPath;
                })
                .Verifiable();

            var sut = new CleanupInstaller(applicationStatusInfoMock.Object, fileSystemUtilsMock.Object)
            {
                Package = new PackageInfo(new InstallerInfo()),
            };

            // act
            sut.ProcessFolder(paths.Item1);
            sut.Install();

            // assert
            Assert.IsTrue(sut.Completed);
            fileSystemUtilsMock.Verify();
            Assert.AreEqual(paths.Item2, actualPath);
        }

        private static IEnumerable<string> InvalidPathsTestCaseSource()
        {
            yield return null;

            var nonSlashedPaths = new[]
            {
               string.Empty,
               " ",
               ".",
               "..",
               "...",
            };

            foreach (var path in nonSlashedPaths)
            {
                yield return path;
                yield return AddTrailingSlash(path);
                yield return Alternate(AddTrailingSlash(path));
            }

            var relativeSlashedPaths = new[]
            {
                "../wwwroot",
                "../wwwroot/..",
                "../wwwroot/dir",
                "Providers/../..",
                "dir/../dir",
                "dir/../../wwwroot/dir",
            };

            foreach (var path in relativeSlashedPaths)
            {
                yield return path;
                yield return Alternate(path);
                yield return MakeAbsolute(path);
                yield return Alternate(MakeAbsolute(path));
                yield return AddTrailingSlash(path);
                yield return Alternate(AddTrailingSlash(path));
                yield return AddTrailingSlash(MakeAbsolute(path));
                yield return Alternate(AddTrailingSlash(MakeAbsolute(path)));
            }

            var absoluteSlashedPaths = new[]
            {
                "C:",
                wwwroot,
                wwwroot + "../wwwroot/dir",
                "//",
                "//fileshare",
                "file://C:/Windows",
                "http://dnndev.me",
            };

            foreach (var path in absoluteSlashedPaths)
            {
                yield return path;
                yield return Alternate(path);
                yield return AddTrailingSlash(path);
                yield return Alternate(AddTrailingSlash(path));
            }
        }

        private static IEnumerable<Tuple<string, string>> ValidPathsTestCaseSource()
        {
            var expectedPath = Alternate(wwwroot + "/dir");

            yield return Tuple.Create("dir", expectedPath);
            yield return Tuple.Create(AddTrailingSlash("dir"), expectedPath);
            yield return Tuple.Create(Alternate(AddTrailingSlash("dir")), expectedPath);

            var relativeSlashedPaths = new[]
            {
                "./dir",
            };

            foreach (var path in relativeSlashedPaths)
            {
                yield return Tuple.Create(path, expectedPath);
                yield return Tuple.Create(Alternate(path), expectedPath);
                yield return Tuple.Create(AddTrailingSlash(path), expectedPath);
                yield return Tuple.Create(Alternate(AddTrailingSlash(path)), expectedPath);
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

        private static string AddTrailingSlash(string path)
        {
            return path + '/';
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
