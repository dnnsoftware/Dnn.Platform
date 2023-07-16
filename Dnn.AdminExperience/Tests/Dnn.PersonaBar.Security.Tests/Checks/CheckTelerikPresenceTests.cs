// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Tests.Checks
{
    using System;
    using System.Linq;

    using Dnn.PersonaBar.Security.Components;
    using Dnn.PersonaBar.Security.Components.Checks;
    using DotNetNuke.Maintenance.Telerik;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CheckTelerikPresenceTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = FakeServiceProvider.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        public void Execute_WhenError_ReturnsUnverified()
        {
            // arrange
            var telerikUtilsMock = new Mock<ITelerikUtils>();

            telerikUtilsMock
                .Setup(x => x.TelerikIsInstalled())
                .Throws<BadImageFormatException>();

            var sut = new CheckTelerikPresence(telerikUtilsMock.Object);

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
            var telerikUtilsMock = new Mock<ITelerikUtils>();

            telerikUtilsMock
                .Setup(x => x.TelerikIsInstalled())
                .Returns(true);

            telerikUtilsMock
                .Setup(x => x.GetAssembliesThatDependOnTelerik())
                .Returns(() => new[] { "bin\\DotNetNuke.Modules.Mod3.dll" });

            telerikUtilsMock
                .SetupGet(x => x.BinPath)
                .Returns("bin");

            var sut = new CheckTelerikPresence(telerikUtilsMock.Object);

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
            var telerikUtilsMock = new Mock<ITelerikUtils>();

            telerikUtilsMock
                .Setup(x => x.TelerikIsInstalled())
                .Returns(true);

            telerikUtilsMock
                .Setup(x => x.GetAssembliesThatDependOnTelerik())
                .Returns(() => new string[0]);

            var sut = new CheckTelerikPresence(telerikUtilsMock.Object);

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
            var telerikUtilsMock = new Mock<ITelerikUtils>();

            telerikUtilsMock
                .Setup(x => x.TelerikIsInstalled())
                .Returns(false);

            var sut = new CheckTelerikPresence(telerikUtilsMock.Object);

            // act
            var result = sut.Execute();

            // assert
            Assert.AreEqual(SeverityEnum.Pass, result.Severity);
            Assert.AreEqual(0, result.Notes.Count());
        }
    }
}
