// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.ConfigConsole.Tests;

using System;
using System.Collections.Generic;
using System.Linq;

using Dnn.PersonaBar.ConfigConsole.Components;
using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

/// <summary><see cref="ConfigConsoleController"/> unit tests.</summary>
[TestFixture]
public class ConfigConsoleControllerTests
{
    private const string GoodConfigXml = "<configuration></configuration>";
    private const string BadConfigXml = "<configuration1></configuration1>";
    private const string BadXml = "<configuration></configuration1>";

    /// <summary>Method that is called immediately before each test is run.</summary>
    [SetUp]
    public static void Setup()
    {
        var applicationStatusInfoMock = new Mock<IApplicationStatusInfo>();

        applicationStatusInfoMock
            .Setup(info => info.ApplicationMapPath)
            .Returns(Environment.CurrentDirectory);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient(container => applicationStatusInfoMock.Object);
        serviceCollection.AddTransient(container => Mock.Of<INavigationManager>());

        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
    }

    /// <summary>Unit test for <see cref="ConfigConsoleController.ValidateConfigFile(string, string)"/>.</summary>
    /// <param name="fileName">File name to test.</param>
    /// <param name="fileContent">File content to test.</param>
    /// <param name="expectedErrorCount">Expected number of validation errors.</param>
    /// <param name="shouldThrow">Whether the method should throw an exception or not.</param>
    [Test]
    [TestCase(ConfigConsoleController.WebConfig, GoodConfigXml, 0, false)]
    [TestCase(ConfigConsoleController.WebConfig, BadConfigXml, 1, false)]
    [TestCase(ConfigConsoleController.WebConfig, BadXml, 0, true)]
    [TestCase("Random.config", GoodConfigXml, 0, false)]
    [TestCase("Random.config", BadConfigXml, 0, false)]
    [TestCase("Random.config", BadXml, 0, false)]
    [TestCase("Random.file", "Random content", 0, false)]

    public void ValidateConfigFile(string fileName, string fileContent, int expectedErrorCount, bool shouldThrow)
    {
        // arrange
        var sut = new ConfigConsoleController();

        Exception exception = null;
        IEnumerable<string> errors = new string[0];

        // act
        try
        {
            errors = sut.ValidateConfigFile(fileName, fileContent);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Assert.Multiple(() =>
        {
            // assert
            Assert.That(exception != null, Is.EqualTo(shouldThrow));
            Assert.That(errors.Count(), Is.EqualTo(expectedErrorCount));
        });
    }
}
