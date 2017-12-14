#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
// by DNN Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Utilities.Mocks;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Entities.Tabs
{
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
            //Arrange
            string invalidType;

            //Act
            var isValid = TabController.IsValidTabName(tabName, out invalidType);

            //Assert
            Assert.IsFalse(isValid, "A forbidden tab name is allowed");
            Assert.AreEqual("InvalidTabName", invalidType, "The invalidType is not the expected one");
        }

        [Test]
        public void IsValidadTab_Returns_False_For_Empty_PageNames()
        {
            //Arrange
            string invalidType;

            //Act
            var isValid = TabController.IsValidTabName("", out invalidType);

            //Assert
            Assert.IsFalse(isValid, "An empty tab name is allowed");
            Assert.AreEqual("EmptyTabName", invalidType, "The invalidType is not the expected one");
        }

        [Test]
        [TestCase("test")]
        [TestCase("mypage")]
        [TestCase("products")]
        public void IsValidadTab_Returns_True_For_Regular_PageNames(string tabName)
        {
            //Arrange
            string invalidType;

            //Act
            var isValid = TabController.IsValidTabName(tabName, out invalidType);

            //Assert
            Assert.IsTrue(isValid, "A regular tab name is not allowed");
            Assert.AreEqual("", invalidType, "The invalidType is not the expected one");
        }
    }
}
