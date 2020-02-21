// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Framework;
using DotNetNuke.Tests.Instance.Utilities;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Framework
{
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
            //Arrange

            //Act
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            //Assert
            Assert.IsTrue(ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired);
        }

        [Test]
        public void NoAjaxAntiForgeryRequestMeansNotRequired()
        {
            //Assert
            Assert.IsFalse(ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired);
        }
    }
}
