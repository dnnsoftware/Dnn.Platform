// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    using System;

    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Subscriptions;
    using DotNetNuke.Services.Social.Subscriptions.Data;
    using DotNetNuke.Services.Social.Subscriptions.Entities;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Builders;
    using DotNetNuke.Tests.Core.Controllers.Messaging.Mocks;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SubscriptionControllerTests
    {
        private Mock<IDataService> mockDataService;
        private Mock<ISubscriptionSecurityController> subscriptionSecurityController;
        private Mock<CachingProvider> mockCacheProvider;

        private SubscriptionController subscriptionController;

        [SetUp]
        public void SetUp()
        {
            // Setup Mocks and Stub
            this.mockDataService = new Mock<IDataService>();
            this.mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
            this.subscriptionSecurityController = new Mock<ISubscriptionSecurityController>();

            DataService.SetTestableInstance(this.mockDataService.Object);
            SubscriptionSecurityController.SetTestableInstance(this.subscriptionSecurityController.Object);

            // Setup SUT
            this.subscriptionController = new SubscriptionController();
        }

        [TearDown]
        public void TearDown()
        {
            DataService.ClearInstance();
            SubscriptionSecurityController.ClearInstance();
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void IsSubscribed_ShouldReturnFalse_IfUserIsNotSubscribed()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader());

            // Act
            var isSubscribed = this.subscriptionController.IsSubscribed(subscription);

            // Assert
            Assert.AreEqual(false, isSubscribed);
        }

        [Test]
        public void IsSubscribed_ShouldReturnFalse_WhenUserDoesNotHavePermissionOnTheSubscription()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            var subscriptionCollection = new[] { subscription };

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(subscriptionCollection));

            this.subscriptionSecurityController
                .Setup(ssc => ssc.HasPermission(It.IsAny<Subscription>())).Returns(false);

            // Act
            var isSubscribed = this.subscriptionController.IsSubscribed(subscription);

            // Assert
            Assert.AreEqual(false, isSubscribed);
        }

        [Test]
        public void IsSubscribed_ShouldReturnTrue_WhenUserHasPermissionOnTheSubscription()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            var subscriptionCollection = new[] { subscription };

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(subscriptionCollection));

            this.subscriptionSecurityController
                .Setup(ssc => ssc.HasPermission(It.IsAny<Subscription>())).Returns(true);

            // Act
            var isSubscribed = this.subscriptionController.IsSubscribed(subscription);

            // Assert
            Assert.AreEqual(true, isSubscribed);
        }

        [Test]
        public void IsSubscribed_ShouldCallDataService_WhenNoError()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId)).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader()).Verifiable();

            // Act
            this.subscriptionController.IsSubscribed(subscription);

            // Assert
            this.mockDataService.Verify(
                ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId), Times.Once);
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()
        {
            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => this.subscriptionController.AddSubscription(null));
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionUserIdPropertyIsNegative()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithUserId(-1)
                .Build();

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => this.subscriptionController.AddSubscription(subscription));
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionSubscriptionTypePropertyIsNegative()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithSubscriptionTypeId(-1)
                .Build();

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => this.subscriptionController.AddSubscription(subscription));
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionObjectKeyIsNull()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithObjectKey(null)
                .Build();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.subscriptionController.AddSubscription(subscription));
        }

        [Test]
        public void AddSubscription_ShouldCallDataService_WhenNoError()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.AddSubscription(
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.ModuleId,
                subscription.TabId,
                subscription.ObjectData)).Verifiable();

            // Act
            this.subscriptionController.AddSubscription(subscription);

            // Assert
            this.mockDataService.Verify(
                ds => ds.AddSubscription(
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.ModuleId,
                subscription.TabId,
                subscription.ObjectData), Times.Once);
        }

        [Test]
        public void AddSubscription_ShouldFilledUpTheSubscriptionIdPropertyOfTheInputSubscriptionEntity_WhenNoError()
        {
            // Arrange
            const int expectedSubscriptionId = 12;

            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.AddSubscription(
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.ModuleId,
                subscription.TabId,
                subscription.ObjectData)).Returns(expectedSubscriptionId);

            // Act
            this.subscriptionController.AddSubscription(subscription);

            // Assert
            Assert.AreEqual(expectedSubscriptionId, subscription.SubscriptionId);
        }

        [Test]
        public void DeleteSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()
        {
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => this.subscriptionController.DeleteSubscription(null));
        }

        [Test]
        public void DeleteSubscriptionType_ShouldCallDeleteSubscriptionDataService_WhenSubscriptionExists()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(new[] { subscription }));

            this.mockDataService.Setup(ds => ds.DeleteSubscription(It.IsAny<int>())).Verifiable();

            // Act
            this.subscriptionController.DeleteSubscription(subscription);

            // Assert
            this.mockDataService.Verify(ds => ds.DeleteSubscription(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void DeleteSubscriptionType_ShouldNotCallDeleteSubscriptionDataService_WhenSubscriptionDoesNotExist()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            this.mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader());

            // Act
            this.subscriptionController.DeleteSubscription(subscription);

            // Assert
            this.mockDataService.Verify(ds => ds.DeleteSubscription(It.IsAny<int>()), Times.Never);
        }
    }
}
