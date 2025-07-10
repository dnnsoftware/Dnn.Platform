// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Caching;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Utilities.Fakes;
using DotNetNuke.Tests.Utilities.Mocks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

[TestFixture]
public class TabCollectionTests
{
    private FakeServiceProvider serviceProvider;

    [SetUp]
    public void SetUp()
    {
        ComponentFactory.Container = new SimpleContainer();
        MockComponentProvider.CreateDataCacheProvider();
        this.serviceProvider = FakeServiceProvider.Setup(services =>
        {
            services.AddSingleton(Mock.Of<IPortalController>());
        });
    }

    [TearDown]
    public void TearDown()
    {
        this.serviceProvider.Dispose();
    }

    [Test]
    public void DNN_13659_WithTabName_NullTabName()
    {
        // Setup
        var tabCollection = new TabCollection();
        tabCollection.Add(new TabInfo { PortalID = 1, TabID = 1 });

        // Act
        var tab = tabCollection.WithTabName("TestName");

        Assert.That(tab, Is.Null);
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
        Assert.That(tab.TabName, Is.EqualTo("TestName2"));
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
        Assert.That(tab, Is.Null);
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
        Assert.That(tab, Is.Null);
    }
}
