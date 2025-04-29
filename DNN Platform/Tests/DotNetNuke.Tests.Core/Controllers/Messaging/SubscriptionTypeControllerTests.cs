// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging;

using System;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Social.Subscriptions;
using DotNetNuke.Services.Social.Subscriptions.Data;
using DotNetNuke.Tests.Core.Controllers.Messaging.Builders;
using DotNetNuke.Tests.Core.Controllers.Messaging.Mocks;
using DotNetNuke.Tests.Utilities.Mocks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

[TestFixture]
public class SubscriptionTypeControllerTests
{
    private const string SubscriptionTypesCacheKey = "DNN_" + DataCache.SubscriptionTypesCacheKey;

    private SubscriptionTypeController subscriptionTypeController;
    private Mock<IDataService> mockDataService;
    private Mock<CachingProvider> mockCacheProvider;
    private Mock<IHostController> mockHostController;

    [SetUp]
    public void SetUp()
    {
        // Setup Mocks and Stub
        this.mockDataService = new Mock<IDataService>();
        this.mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
        this.mockHostController = new Mock<IHostController>();
        this.mockHostController.As<IHostSettingsService>();

        DataService.SetTestableInstance(this.mockDataService.Object);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
        serviceCollection.AddTransient<IHostSettingsService>(container => (IHostSettingsService)this.mockHostController.Object);
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();

        // Setup SUT
        this.subscriptionTypeController = new SubscriptionTypeController();
    }

    [Test]

    public void GetSubscriptionTypes_ShouldCallDataService_WhenNoError()
    {
        // Arrange
        this.mockHostController
            .Setup(c => c.GetString("PerformanceSetting"))
            .Returns("0");

        this.mockDataService
            .Setup(ds => ds.GetSubscriptionTypes())
            .Returns(SubscriptionTypeDataReaderMockHelper.CreateEmptySubscriptionTypeReader())
            .Verifiable();

        // Act
        this.subscriptionTypeController.GetSubscriptionTypes();

        // Assert
        this.mockDataService.Verify(ds => ds.GetSubscriptionTypes(), Times.Once());
    }

    [Test]

    public void GetSubscriptionTypes_ShouldThrowArgumentNullException_WhenPredicateIsNull()
    {
        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => this.subscriptionTypeController.GetSubscriptionTypes(null));
    }

    [Test]

    public void GetSubscriptionType_ShouldThrowArgumentNullException_WhenPredicateIsNull()
    {
        // Act, Assert
        Assert.Throws<ArgumentNullException>(() => this.subscriptionTypeController.GetSubscriptionType(null));
    }

    [Test]
    public void AddSubscriptionType_ShouldThrowArgumentNullException_WhenSubscriptionTypeIsNull()
    {
        // Act, Arrange
        Assert.Throws<ArgumentNullException>(() => this.subscriptionTypeController.AddSubscriptionType(null));
    }

    [Test]
    public void AddSubscriptionType_ShouldFilledUpTheSubscriptionTypeIdPropertyOfTheInputSubscriptionTypeEntity_WhenNoError()
    {
        // Arrange
        const int expectedSubscriptionTypeId = 12;
        var subscriptionType = new SubscriptionTypeBuilder().Build();

        this.mockDataService
            .Setup(ds => ds.AddSubscriptionType(
                subscriptionType.SubscriptionName,
                subscriptionType.FriendlyName,
                subscriptionType.DesktopModuleId))
            .Returns(expectedSubscriptionTypeId);

        // Act
        this.subscriptionTypeController.AddSubscriptionType(subscriptionType);

        // Assert
        Assert.That(subscriptionType.SubscriptionTypeId, Is.EqualTo(expectedSubscriptionTypeId));
    }

    [Test]
    public void AddSubscriptionType_ShouldCleanCache_WhenNoError()
    {
        // Arrange
        this.mockDataService.Setup(ds => ds.AddSubscriptionType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
        this.mockCacheProvider.Setup(cp => cp.Remove(SubscriptionTypesCacheKey)).Verifiable();

        var subscriptionType = new SubscriptionTypeBuilder().Build();

        // Act
        this.subscriptionTypeController.AddSubscriptionType(subscriptionType);

        // Assert
        this.mockCacheProvider.Verify(cp => cp.Remove(SubscriptionTypesCacheKey), Times.Once());
    }

    [Test]
    public void DeleteSubscriptionType_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionTypeIdIsNegative()
    {
        // Arrange
        var subscriptionType = new SubscriptionTypeBuilder()
            .WithSubscriptionTypeId(-1)
            .Build();

        // Act, Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => this.subscriptionTypeController.DeleteSubscriptionType(subscriptionType));
    }

    [Test]
    public void DeleteSubscriptionType_ShouldThrowNullArgumentException_WhenSubscriptionTypeIsNull()
    {
        // Act, Assert
        Assert.Throws<ArgumentNullException>(() => this.subscriptionTypeController.DeleteSubscriptionType(null));
    }

    [Test]
    public void DeleteSubscriptionType_ShouldCallDataService_WhenNoError()
    {
        // Arrange
        var subscriptionType = new SubscriptionTypeBuilder().Build();

        this.mockDataService
            .Setup(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId))
            .Verifiable();

        // Act
        this.subscriptionTypeController.DeleteSubscriptionType(subscriptionType);

        // Assert
        this.mockDataService.Verify(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId), Times.Once());
    }

    [Test]
    public void DeleteSubscriptionType_ShouldCleanCache_WhenNoError()
    {
        // Arrange
        var subscriptionType = new SubscriptionTypeBuilder().Build();

        this.mockDataService.Setup(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId));
        this.mockCacheProvider.Setup(cp => cp.Remove(SubscriptionTypesCacheKey)).Verifiable();

        // Act
        this.subscriptionTypeController.DeleteSubscriptionType(subscriptionType);

        // Assert
        this.mockCacheProvider.Verify(cp => cp.Remove(SubscriptionTypesCacheKey), Times.Once());
    }

    [TearDown]
    public void TearDown()
    {
        Globals.DependencyProvider = null;
        DataService.ClearInstance();
        MockComponentProvider.ResetContainer();
    }
}
