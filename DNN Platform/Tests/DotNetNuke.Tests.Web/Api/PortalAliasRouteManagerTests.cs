#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.Api;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class PortalAliasRouteManagerTests
    {

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
            PortalAliasController.ClearInstance();
            TestableGlobals.ClearInstance();
        }


        [Test]
        [TestCase("mfn", "url", 0, "API/mfn/url")]
        [TestCase("mfn", "url", 1, "{prefix0}/API/mfn/url")]
        [TestCase("mfn", "url", 2, "{prefix0}/{prefix1}/API/mfn/url")]
        [TestCase("fee/foo", "{contoller}/{action}/{id}", 4, "{prefix0}/{prefix1}/{prefix2}/{prefix3}/API/fee/foo/{contoller}/{action}/{id}")]
        public void GetRouteUrl(string moduleFolderName, string url, int count, string expected)
        {
            //Arrange


            //Act
            string result = new PortalAliasRouteManager().GetRouteUrl(moduleFolderName, url, count);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase("", 0, typeof(ArgumentException), "moduleFolderName should be required")]
        [TestCase("name", -1, typeof(ArgumentOutOfRangeException), "count should be >= 0")]
        public void GetRouteUrlThrowsOnBadArguments(string moduleFolderName, int count, Type expectedException, string message)
        {
            //Arrange


            //Act
            try
            {
                new PortalAliasRouteManager().GetRouteUrl(moduleFolderName, "url", count);
            }
            catch(Exception e)
            {
                if(e.GetType() == expectedException)
                {
                    Assert.Pass();
                    return;
                }
            }

            //Assert
            Assert.Fail(message);
        }

        [Test]
        public void ParentPortalOnVirtualDirReturnsAnEmptyPrefix()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            var portals = new ArrayList {new PortalInfo {PortalID = 1}};
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalAliasController = new Mock<IPortalAliasController>();
            mockPortalAliasController.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {new PortalAliasInfo {HTTPAlias = "valid.lvh.me/vdir"}});
            PortalAliasController.SetTestableInstance(mockPortalAliasController.Object);

            var mockGlobals = new Mock<IGlobals>();
            mockGlobals.Setup(x => x.ApplicationPath).Returns("/vdir");
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            //Act
            List<int> prefixes = new PortalAliasRouteManager().GetRoutePrefixCounts().ToList();

            //Assert
            CollectionAssert.AreEquivalent(new[] {0}, prefixes);
        }

        [Test]
        public void SingleParentPortalReturnsAnEmptyPrefix()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            var portals = new ArrayList {new PortalInfo {PortalID = 1}};
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalAliasController = new Mock<IPortalAliasController>();
            mockPortalAliasController.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {new PortalAliasInfo {HTTPAlias = "valid.lvh.me"}});
            PortalAliasController.SetTestableInstance(mockPortalAliasController.Object);

            var mockGlobals = new Mock<IGlobals>();
            mockGlobals.Setup(x => x.ApplicationPath).Returns("");
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            //Act
            List<int> prefixes = new PortalAliasRouteManager().GetRoutePrefixCounts().ToList();

            //Assert
            CollectionAssert.AreEquivalent(new[] {0}, prefixes);
        }

        [Test]
        public void PrefixCountsAreCached()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            var portals = new ArrayList { new PortalInfo { PortalID = 1 } };
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalAliasController = new Mock<IPortalAliasController>();
            mockPortalAliasController.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[] { new PortalAliasInfo { HTTPAlias = "valid.lvh.me" } });
            PortalAliasController.SetTestableInstance(mockPortalAliasController.Object);

            var mockGlobals = new Mock<IGlobals>();
            mockGlobals.Setup(x => x.ApplicationPath).Returns("");
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            //Act
            var parm = new PortalAliasRouteManager();
            parm.GetRoutePrefixCounts();
            parm.GetRoutePrefixCounts();

            //Assert
            mockPortalController.Verify(x => x.GetPortals(), Times.Once());
        }

        [Test]
        public void PrefixCountsCacheCanBeCleared()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            var portals = new ArrayList { new PortalInfo { PortalID = 1 } };
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalAliasController = new Mock<IPortalAliasController>();
            mockPortalAliasController.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[] { new PortalAliasInfo { HTTPAlias = "valid.lvh.me" } });
            PortalAliasController.SetTestableInstance(mockPortalAliasController.Object);

            var mockGlobals = new Mock<IGlobals>();
            mockGlobals.Setup(x => x.ApplicationPath).Returns("");
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            //Act
            var parm = new PortalAliasRouteManager();
            parm.GetRoutePrefixCounts();
            parm.ClearCachedData();
            parm.GetRoutePrefixCounts();

            //Assert
            mockPortalController.Verify(x => x.GetPortals(), Times.Exactly(2));
        }

        [Test]
        public void VirtralDirWithChildPortalHasABlankAndASinglePrefix()
        {
            //Arrange
            var mockPortalController = new Mock<IPortalController>();
            var portals = new ArrayList {new PortalInfo {PortalID = 1}};
            mockPortalController.Setup(x => x.GetPortals()).Returns(portals);
            PortalController.SetTestableInstance(mockPortalController.Object);

            var mockPortalAliasController = new Mock<IPortalAliasController>();
            mockPortalAliasController.Setup(x => x.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(new[]
                {
                    new PortalAliasInfo {HTTPAlias = "valid.lvh.me/vdir"},
                    new PortalAliasInfo {HTTPAlias = "valid.lvh.me/vdir/child"}
                });
            PortalAliasController.SetTestableInstance(mockPortalAliasController.Object);

            var mockGlobals = new Mock<IGlobals>();
            mockGlobals.Setup(x => x.ApplicationPath).Returns("/vdir");
            TestableGlobals.SetTestableInstance(mockGlobals.Object);

            //Act
            List<int> prefixes = new PortalAliasRouteManager().GetRoutePrefixCounts().ToList();

            //Assert
            CollectionAssert.AreEqual(new[] {1, 0}, prefixes);
        }

        [Test]
        [TestCase("mfn", "name", 0, "mfn-name-0")]
        [TestCase("mfn", "", 1, "mfn--1")]
        [TestCase("first", "second", 99, "first-second-99")]
        public void GetRouteNameHashesNameInCorrectFormat(string moduleFolderName, string routeName, int count, string expected)
        {
            //Arrange
            

            //Act
            var result = new PortalAliasRouteManager().GetRouteName(moduleFolderName, routeName, count);

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase("mfn", "name", "ce.lvh.me", "mfn-name-0")]
        [TestCase("mfn", "", "ce.lvh.me/child", "mfn--1")]
        [TestCase("first", "second", "ce.lvh.me/child1/child2/child3/child4/child5", "first-second-5")]
        public void GetRouteNameWithPortalAliasInfoHashesNameInCorrectFormat(string moduleFolderName, string routeName, string httpAlias, string expected)
        {
            //Arrange


            //Act
            var result = new PortalAliasRouteManager().GetRouteName(moduleFolderName, routeName, new PortalAliasInfo {HTTPAlias = httpAlias});

            //Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        [ExpectedException(typeof(ArgumentException))]
        public void GetRouteNameThrowsOnEmptyModuleFolderName(string moduleFolderName)
        {
            //Arrange
            

            //Act
            new PortalAliasRouteManager().GetRouteName(moduleFolderName, "", 0);

            //Assert
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetRouteNameThrowsOnCountLessThan0()
        {
            //Arrange
            

            //Act
            new PortalAliasRouteManager().GetRouteName("foo", "", -1);

            //Assert
        }

        [Test]
        public void GetAllRouteValuesWorksWithNullRouteValues   ()
        {
            //Arrange
            

            //Act
            new PortalAliasRouteManager().GetAllRouteValues(new PortalAliasInfo {HTTPAlias = ""}, null);

            //Assert
            Assert.Pass();
        }

        [Test]
        public void GetAllRouteValuesPreservesPassedInRouteValues()
        {
            //Arrange
            

            //Act
            var result = new PortalAliasRouteManager().GetAllRouteValues(new PortalAliasInfo {HTTPAlias = ""},
                                                               new {value1 = 1, value2 = 2});

            //Assert
            var expected = new Dictionary<string, object> {{"value1", 1}, {"value2", 2}};
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void GetAllRouteValuesExtractsChildPortalParams()
        {
            //Arrange
            

            //Act
            var result =
                new PortalAliasRouteManager().GetAllRouteValues(new PortalAliasInfo {HTTPAlias = "ce.lvh.me/child"},
                                                                   null);

            //Assert
            var expected = new Dictionary<string, object> { { "prefix0", "child" } };
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void GetAllRouteValuesExtractsManyChildPortalParamsAndPreservesRouteValues()
        {
            //Arrange


            //Act
            var result =
                new PortalAliasRouteManager().GetAllRouteValues(new PortalAliasInfo { HTTPAlias = "ce.lvh.me/child0/child1/child2/child3" },
                                                                   new {value1 = 1, value2 = 2});

            //Assert
            var expected = new Dictionary<string, object> { { "prefix0", "child0" }, { "prefix1", "child1" }, { "prefix2", "child2" }, { "prefix3", "child3" }, { "value1", 1}, {"value2", 2} };
            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}