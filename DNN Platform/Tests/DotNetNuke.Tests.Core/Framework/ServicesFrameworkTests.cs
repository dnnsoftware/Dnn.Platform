// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Framework;

using System;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Framework;
using DotNetNuke.Tests.Instance.Utilities;
using DotNetNuke.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

public class ServicesFrameworkTests
{
    [SetUp]
    public void Setup()
    {
        var serviceCollection = new ServiceCollection();
        var mockApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
        mockApplicationStatusInfo.Setup(info => info.Status).Returns(UpgradeStatus.Install);
        serviceCollection.AddTransient<IApplicationStatusInfo>(container => mockApplicationStatusInfo.Object);
        serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();

        HttpContextHelper.RegisterMockHttpContext();
        var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", "c:\\");
        simulator.SimulateRequest(new Uri("http://localhost/dnn/Default.aspx"));
    }

    [TearDown]
    public void TearDown()
    {
        Globals.DependencyProvider = null;
        UnitTestHelper.ClearHttpContext();
    }

    [Test]
    public void RequestingAjaxAntiForgeryIsNoted()
    {
        // Arrange

        // Act
        ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

        // Assert
        Assert.That(ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired, Is.True);
    }

    [Test]
    public void NoAjaxAntiForgeryRequestMeansNotRequired()
    {
        // Assert
        Assert.That(ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired, Is.False);
    }
}
