// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Framework
{
    using System;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    public class ServicesFrameworkTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = FakeServiceProvider.Setup(services => services.AddSingleton(Mock.Of<IPortalController>()));

            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", "c:\\");
            simulator.SimulateRequest(new Uri("http://localhost/dnn/Default.aspx"));
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        public void RequestingAjaxAntiForgeryIsNoted()
        {
            var servicesFramework = new ServicesFrameworkImpl(
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>());
            servicesFramework.RequestAjaxAntiForgerySupport();

            Assert.That(servicesFramework.IsAjaxAntiForgerySupportRequired, Is.True);
        }

        [Test]
        public void NoAjaxAntiForgeryRequestMeansNotRequired()
        {
            var servicesFramework = new ServicesFrameworkImpl(
                Mock.Of<IApplicationStatusInfo>(),
                Mock.Of<IEventLogger>());
            Assert.That(servicesFramework.IsAjaxAntiForgerySupportRequired, Is.False);
        }
    }
}
