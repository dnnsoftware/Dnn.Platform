namespace Dnn.PersonaBar.Security.Tests.Checks
{
    using System;
    using System.IO;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.Security.Helper;
    using Dnn.PersonaBar.Security.Components;
    using Dnn.PersonaBar.Security.Components.Checks;
    using DotNetNuke.Abstractions.Application;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CheckTelerikPresenceTests
    {
        private static readonly string AppPath = @"C:\inetpub\dnn.dnndev.me";

        private static readonly string[] AssemblyList = new[]
        {
            "Telerik.Web.UI.Skins.dll",
            "DotNetNuke.Website.Deprecated.dll",
            "DotNetNuke.Web.Deprecated.dll",
            "DotNetNuke.Modules.DigitalAssets.dll",
            "DotNetNuke.Modules.Mod1.dll",
            "DotNetNuke.Modules.Mod2.dll",
            "DotNetNuke.Modules.Mod3.dll",
        };

        private static readonly string[] AssemblyFullPathList = AssemblyList
            .Select(fileName => Path.Combine(AppPath, "bin", fileName))
            .ToArray();

        [Test]
        public void Execute_WhenError_ReturnsUnverified()
        {
            // arrange
            var applicationStatusInfoMock = SetupApplicationStatusInfoMock();

            var fileHelperMock = SetupFileHelperMock(installed: true);

            fileHelperMock
                .Setup(x => x.GetReferencedAssemblyNames(It.IsAny<string>()))
                .Throws<BadImageFormatException>();

            var sut = new CheckTelerikPresence(applicationStatusInfoMock.Object, fileHelperMock.Object);

            // act
            var result = sut.Execute();

            // assert
            Assert.AreEqual(SeverityEnum.Unverified, result.Severity);
            Assert.AreEqual(1, result.Notes.Count());
            Assert.IsTrue(result.Notes.First() == "An internal error occurred. See logs for details.");
        }

        [Test]
        public void Execute_WhenInstalledAndUsed_ReturnsInstalledAndUsed()
        {
            // arrange
            var applicationStatusInfoMock = SetupApplicationStatusInfoMock();

            var fileHelperMock = SetupFileHelperMock(installed: true);

            fileHelperMock
                .Setup(x => x.GetReferencedAssemblyNames(It.IsAny<string>()))
                .Returns<string>(assemblyFilePath =>
                {
                    var fileName = Path.GetFileName(assemblyFilePath);
                    switch (fileName)
                    {
                        case "DotNetNuke.Modules.Mod3.dll":
                            return new[] { "DotNetNuke.dll", CheckTelerikPresence.TelerikWebUIFileName };
                        default:
                            return new[] { "DotNetNuke.dll" };
                    }
                });

            var sut = new CheckTelerikPresence(applicationStatusInfoMock.Object, fileHelperMock.Object);

            // act
            var result = sut.Execute();

            // assert
            Assert.AreEqual(SeverityEnum.Failure, result.Severity);
            Assert.AreEqual(1, result.Notes.Count());
            Assert.IsTrue(result.Notes.First().Contains("* DotNetNuke.Modules.Mod3.dll"));
        }

        [Test]
        public void Execute_WhenInstalledButNotUsed_ReturnsInstalledButNotUsed()
        {
            // arrange
            var applicationStatusInfoMock = SetupApplicationStatusInfoMock();

            var fileHelperMock = SetupFileHelperMock(installed: true);

            fileHelperMock
                .Setup(x => x.GetReferencedAssemblyNames(It.IsAny<string>()))
                .Returns(new[] { "DotNetNuke.dll" });

            var sut = new CheckTelerikPresence(applicationStatusInfoMock.Object, fileHelperMock.Object);

            // act
            var result = sut.Execute();

            // assert
            Assert.AreEqual(SeverityEnum.Failure, result.Severity);
            Assert.AreEqual(1, result.Notes.Count());
        }

        [Test]
        public void Execute_WhenNotInstalled_ReturnsNotInstalled()
        {
            // arrange
            var applicationStatusInfoMock = SetupApplicationStatusInfoMock();

            var fileHelperMock = SetupFileHelperMock(installed: false);

            var sut = new CheckTelerikPresence(applicationStatusInfoMock.Object, fileHelperMock.Object);

            // act
            var result = sut.Execute();

            // assert
            Assert.AreEqual(SeverityEnum.Pass, result.Severity);
            Assert.AreEqual(0, result.Notes.Count());
        }

        private static bool FileIsTelerik(string path)
        {
            return Path.GetFileName(path) == CheckTelerikPresence.TelerikWebUIFileName;
        }

        private static Mock<IApplicationStatusInfo> SetupApplicationStatusInfoMock()
        {
            var mock = new Mock<IApplicationStatusInfo>();

            mock.SetupGet(x => x.ApplicationMapPath)
                .Returns(AppPath);

            return mock;
        }

        private static Mock<IFileHelper> SetupFileHelperMock(bool installed)
        {
            var mock = new Mock<IFileHelper>();

            mock.Setup(x => x.FileExists(It.Is<string>(path => FileIsTelerik(path))))
                .Returns(installed);

            mock.Setup(x => x.DirectoryGetFiles(It.IsAny<string>(), "*.dll", SearchOption.AllDirectories))
                .Returns(AssemblyFullPathList);

            return mock;
        }
    }
}
