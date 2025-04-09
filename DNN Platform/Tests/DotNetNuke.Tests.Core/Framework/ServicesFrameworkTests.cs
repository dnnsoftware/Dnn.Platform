// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Framework
{
    using System;

    using DotNetNuke.Framework;
    using DotNetNuke.Tests.Utilities.Fakes;

    using NUnit.Framework;

    public class ServicesFrameworkTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            this.serviceProvider = FakeServiceProvider.Setup();

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
}
