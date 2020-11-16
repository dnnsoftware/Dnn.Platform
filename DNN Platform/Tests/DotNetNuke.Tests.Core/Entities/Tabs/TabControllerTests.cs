// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Tabs
{
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Mocks;
    using NUnit.Framework;

    [TestFixture]
    public class TabControllerTests
    {
        [SetUp]
        public void SetUp()
        {
            MockComponentProvider.CreateDataProvider();
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            TabController.ClearInstance();
        }

        [Test]
        [TestCase("Lpt1")]
        [TestCase("Lpt2")]
        [TestCase("Lpt3")]
        [TestCase("Lpt4")]
        [TestCase("Lpt5")]
        [TestCase("Lpt6")]
        [TestCase("Lpt7")]
        [TestCase("Lpt8")]
        [TestCase("Lpt9")]
        [TestCase("Com1")]
        [TestCase("Com2")]
        [TestCase("Com3")]
        [TestCase("Com4")]
        [TestCase("Com5")]
        [TestCase("Com6")]
        [TestCase("Com7")]
        [TestCase("Com8")]
        [TestCase("Com9")]
        [TestCase("Aux")]
        [TestCase("Con")]
        [TestCase("Nul")]
        [TestCase("SiteMap")]
        [TestCase("Linkclick")]
        [TestCase("KeepAlive")]
        [TestCase("Default")]
        [TestCase("ErrorPage")]
        [TestCase("Login")]
        [TestCase("Register")]
        public void IsValidadTab_Returns_False_For_Forbidden_PageNames(string tabName)
        {
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(tabName, out invalidType);

            // Assert
            Assert.IsFalse(isValid, "A forbidden tab name is allowed");
            Assert.AreEqual("InvalidTabName", invalidType, "The invalidType is not the expected one");
        }

        [Test]
        public void IsValidadTab_Returns_False_For_Empty_PageNames()
        {
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(string.Empty, out invalidType);

            // Assert
            Assert.IsFalse(isValid, "An empty tab name is allowed");
            Assert.AreEqual("EmptyTabName", invalidType, "The invalidType is not the expected one");
        }

        [Test]
        [TestCase("test")]
        [TestCase("mypage")]
        [TestCase("products")]
        public void IsValidadTab_Returns_True_For_Regular_PageNames(string tabName)
        {
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(tabName, out invalidType);

            // Assert
            Assert.IsTrue(isValid, "A regular tab name is not allowed");
            Assert.AreEqual(string.Empty, invalidType, "The invalidType is not the expected one");
        }
    }
}
