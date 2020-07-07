// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Framework
{
    using System;

    using DotNetNuke.Framework;
    using DotNetNuke.Tests.Instance.Utilities;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    public class ServicesFrameworkTests
    {
        [SetUp]
        public void Setup()
        {
            HttpContextHelper.RegisterMockHttpContext();
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", "c:\\");
            simulator.SimulateRequest(new Uri("http://localhost/dnn/Default.aspx"));
        }

        [TearDown]
        public void TearDown()
        {
            UnitTestHelper.ClearHttpContext();
        }

        [Test]
        public void RequestingAjaxAntiForgeryIsNoted()
        {
            // Arrange

            // Act
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            // Assert
            Assert.IsTrue(ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired);
        }

        [Test]
        public void NoAjaxAntiForgeryRequestMeansNotRequired()
        {
            // Assert
            Assert.IsFalse(ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired);
        }
    }
}
