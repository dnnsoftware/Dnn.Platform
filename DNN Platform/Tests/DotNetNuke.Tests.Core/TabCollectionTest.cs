#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Utilities.Mocks;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Providers.Caching
{
    /// <summary>
    ///   Summary description for DataCacheTests
    /// </summary>
    [TestFixture]
    public class TabCollectionsTest
    {
        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            MockComponentProvider.CreateDataCacheProvider();
        }


        [Test]
        public void DNN_13659_WithTabName_NullTabName()
        {
            //Setup
            var tabCollection = new TabCollection();
            tabCollection.Add(new TabInfo {PortalID = 1, TabID = 1});

            //Act
            var tab = tabCollection.WithTabName("TestName");

            Assert.IsNull(tab);
        }

        [Test]
        public void WithTabName_Match()
        {
            //Setup
            var tabCollection = new TabCollection();
            tabCollection.Add(new TabInfo {TabName = "TestName1", PortalID = 1, TabID = 1});
            tabCollection.Add(new TabInfo {TabName = "TestName2", PortalID = 1, TabID = 2});
            tabCollection.Add(new TabInfo {TabName = "TestName3", PortalID = 1, TabID = 3});

            //Act
            var tab = tabCollection.WithTabName("TestName2");

            //Assert
            Assert.AreEqual("TestName2", tab.TabName);
        }

        [Test]
        public void WithTabName_NoMatch()
        {
            //Setup
            var tabCollection = new TabCollection();
            tabCollection.Add(new TabInfo {TabName = "TestName1", PortalID = 1, TabID = 1});
            tabCollection.Add(new TabInfo {TabName = "TestName2", PortalID = 1, TabID = 2});
            tabCollection.Add(new TabInfo {TabName = "TestName3", PortalID = 1, TabID = 3});

            //Act
            var tab = tabCollection.WithTabName("NO_MATCH");

            //Assert
            Assert.IsNull(tab);
        }

        [Test]
        public void WithTabName_Empty()
        {
            //Setup
            var tabCollection = new TabCollection();
            tabCollection.Add(new TabInfo {TabName = "TestName1", PortalID = 1, TabID = 1});
            tabCollection.Add(new TabInfo {TabName = "TestName2", PortalID = 1, TabID = 2});
            tabCollection.Add(new TabInfo {TabName = "TestName3", PortalID = 1, TabID = 3});

            //Act
            var tab = tabCollection.WithTabName(String.Empty);

            //Assert
            Assert.IsNull(tab);
        }
    }
}