// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.DependencyInjection;

using System;
using System.Web.Http.Filters;

using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Common.Utilities;
using DotNetNuke.DependencyInjection.Extensions;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

[TestFixture]
public partial class BuildUpExtensionsTests
{
    // The event logger is used in the test Filters
    private IEventLogger eventLogger;
    private IServiceProvider container;

    [SetUp]
    public void Setup()
    {
        CBO.SetTestableInstance(new MockCBO());

        this.eventLogger = Mock.Of<IEventLogger>();

        var mockContainer = new Mock<IServiceProvider>();
        mockContainer.Setup(provider => provider.GetService(typeof(IEventLogger))).Returns(this.eventLogger);

        this.container = mockContainer.Object;
    }

    [TearDown]
    public void TearDown()
    {
        this.eventLogger = null;
        this.container = null;
        CBO.ClearInstance();
    }

    [Test]
    public void CheckParameter_ContainerIsNullTest()
    {
        var container = default(IServiceProvider);

        // This should invoke without exceptions
        container.BuildUp(Mock.Of<IFilter>());
    }

    [Test]
    public void CheckParameter_Filter_IsNullTest()
    {
        var container = Mock.Of<IServiceProvider>();

        // This should invoke without exceptions
        container.BuildUp(null);
    }

    [Test]
    public void BuildUp_PrivateProperty_Test()
    {
        var filter = new PrivateFilterAttribute();

        this.container.BuildUp(filter);
        filter.OnActionExecuted(null);

        this.VerifyEventLoggerInvoked();
    }

    [Test]
    public void BuildUp_ProtectedProperty_Test()
    {
        var filter = new ProtectedFilterAttribute();

        this.container.BuildUp(filter);
        filter.OnActionExecuted(null);

        this.VerifyEventLoggerInvoked();

    }

    [Test]
    public void BuildUp_ProtectedInternalProperty_Test()
    {
        var filter = new ProtectedInternalFilterAttribute();

        this.container.BuildUp(filter);
        filter.OnActionExecuted(null);

        this.VerifyEventLoggerInvoked();
    }

    [Test]
    public void BuildUp_InternalProperty_Test()
    {
        var filter = new InternalFilterAttribute();

        this.container.BuildUp(filter);
        filter.OnActionExecuted(null);

        this.VerifyEventLoggerInvoked();
    }

    [Test]
    public void BuildUp_NoDependencyAttribute_Test()
    {
        var filter = new PublicNoDependencyAttributeFilterAttribute();

        this.container.BuildUp(filter);
        Assert.Throws<NullReferenceException>(() => filter.OnActionExecuted(null));

        this.VerifyEventLoggerInvoked(Times.Never());
    }

    [Test]
    public void BuildUp_NoSetter_Test()
    {
        var filter = new NoSetFilterAttribute();

        this.container.BuildUp(filter);
        Assert.Throws<NullReferenceException>(() => filter.OnActionExecuted(null));

        this.VerifyEventLoggerInvoked(Times.Never());
    }

    private void VerifyEventLoggerInvoked() => this.VerifyEventLoggerInvoked(Times.Once());

    private void VerifyEventLoggerInvoked(Times times) =>
        Mock.Get(this.eventLogger)
            .Verify(logger => logger.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogType>()), times);

}
