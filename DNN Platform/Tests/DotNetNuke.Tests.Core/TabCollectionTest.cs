// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Caching
{
    using System;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Mocks;
    using NUnit.Framework;

    /// <summary>
    ///   Summary description for DataCacheTests.
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
            // Setup
            var tabCollection = new TabCollection();
            tabCollection.Add(new TabInfo { PortalID = 1, TabID = 1 });

            // Act
            var tab = tabCollection.WithTabName("TestName");

            Assert.IsNull(tab);
        }

        [Test]
        public void WithTabName_Match()
        {
            // Setup
            var tabCollection = new TabCollection();
            tabCollection.Add(new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1 });
            tabCollection.Add(new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2 });
            tabCollection.Add(new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3 });

            // Act
            var tab = tabCollection.WithTabName("TestName2");

            // Assert
            Assert.AreEqual("TestName2", tab.TabName);
        }

        [Test]
        public void WithTabName_NoMatch()
        {
            // Setup
            var tabCollection = new TabCollection();
            tabCollection.Add(new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1 });
            tabCollection.Add(new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2 });
            tabCollection.Add(new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3 });

            // Act
            var tab = tabCollection.WithTabName("NO_MATCH");

            // Assert
            Assert.IsNull(tab);
        }

        [Test]
        public void WithTabName_Empty()
        {
            // Setup
            var tabCollection = new TabCollection();
            tabCollection.Add(new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1 });
            tabCollection.Add(new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2 });
            tabCollection.Add(new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3 });

            // Act
            var tab = tabCollection.WithTabName(string.Empty);

            // Assert
            Assert.IsNull(tab);
        }
    }
}
