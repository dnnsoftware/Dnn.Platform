namespace Dnn.PersonaBar.Security.Tests.Checks;

using System;
using System.Linq;

using Dnn.PersonaBar.Security.Components;
using Dnn.PersonaBar.Security.Components.Checks;
using DotNetNuke.Common.Internal;
using DotNetNuke.Maintenance.Telerik;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CheckTelerikPresenceTests
{
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

        Assert.Multiple(() =>
        {
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Unverified));
            Assert.That(result.Notes.Count(), Is.EqualTo(1));
            Assert.That(result.Notes.First() == "An internal error occurred. See logs for details.", Is.True);
        });
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

        Assert.Multiple(() =>
        {
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Failure));
            Assert.That(result.Notes.Count(), Is.EqualTo(1));
            Assert.That(result.Notes.First().Contains("* DotNetNuke.Modules.Mod3.dll"), Is.True);
        });
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

        Assert.Multiple(() =>
        {
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Failure));
            Assert.That(result.Notes.Count(), Is.EqualTo(1));
        });
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

        Assert.Multiple(() =>
        {
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
            Assert.That(result.Notes.Count(), Is.EqualTo(0));
        });
    }
}
