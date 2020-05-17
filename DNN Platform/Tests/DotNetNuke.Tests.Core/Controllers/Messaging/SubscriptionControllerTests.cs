﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
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
            mockDataService = new Mock<IDataService>();
            mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
            subscriptionSecurityController = new Mock<ISubscriptionSecurityController>();

            DataService.SetTestableInstance(mockDataService.Object);
            SubscriptionSecurityController.SetTestableInstance(subscriptionSecurityController.Object);

            // Setup SUT
            subscriptionController = new SubscriptionController();
        }
        
        [TearDown]
        public void TearDown()
        {
            DataService.ClearInstance();
            SubscriptionSecurityController.ClearInstance();
            MockComponentProvider.ResetContainer();
        }

        #region IsSubscribed method tests
        [Test]
        public void IsSubscribed_ShouldReturnFalse_IfUserIsNotSubscribed()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader());
            
            //Act
            var isSubscribed = subscriptionController.IsSubscribed(subscription);

            // Assert
            Assert.AreEqual(false, isSubscribed);
        }

        [Test]
        public void IsSubscribed_ShouldReturnFalse_WhenUserDoesNotHavePermissionOnTheSubscription()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            var subscriptionCollection = new[] {subscription};

            mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(subscriptionCollection));

            subscriptionSecurityController
                .Setup(ssc => ssc.HasPermission(It.IsAny<Subscription>())).Returns(false);

            //Act
            var isSubscribed = subscriptionController.IsSubscribed(subscription);

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

            mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(subscriptionCollection));

            subscriptionSecurityController
                .Setup(ssc => ssc.HasPermission(It.IsAny<Subscription>())).Returns(true);

            //Act
            var isSubscribed = subscriptionController.IsSubscribed(subscription);

            // Assert
            Assert.AreEqual(true, isSubscribed);
        }

        [Test]
        public void IsSubscribed_ShouldCallDataService_WhenNoError()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId)).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader()).Verifiable();

            //Act
            subscriptionController.IsSubscribed(subscription);

            // Assert
            mockDataService.Verify(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId), Times.Once);
        }
        #endregion

        #region AddSubscription method tests
        [Test]
        public void AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()
        {
            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => subscriptionController.AddSubscription(null));
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionUserIdPropertyIsNegative()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithUserId(-1)
                .Build();

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => subscriptionController.AddSubscription(subscription));
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionSubscriptionTypePropertyIsNegative()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithSubscriptionTypeId(-1)
                .Build();

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => subscriptionController.AddSubscription(subscription));
        }

        [Test]
        public void AddSubscription_ShouldThrowArgumentNullException_WhenSubscriptionObjectKeyIsNull()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .WithObjectKey(null)
                .Build();

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => subscriptionController.AddSubscription(subscription));
        }
        
        [Test]
        public void AddSubscription_ShouldCallDataService_WhenNoError()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();
            
            mockDataService.Setup(ds => ds.AddSubscription(
                subscription.UserId, 
                subscription.PortalId, 
                subscription.SubscriptionTypeId, 
                subscription.ObjectKey, 
                subscription.Description, 
                subscription.ModuleId, 
                subscription.TabId,
                subscription.ObjectData)).Verifiable();

            //Act
            subscriptionController.AddSubscription(subscription);

            // Assert
            mockDataService.Verify(ds => ds.AddSubscription(
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

            mockDataService.Setup(ds => ds.AddSubscription(
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.ModuleId,
                subscription.TabId,
                subscription.ObjectData)).Returns(expectedSubscriptionId);

            //Act
            subscriptionController.AddSubscription(subscription);

            // Assert
            Assert.AreEqual(expectedSubscriptionId, subscription.SubscriptionId);
        }
        #endregion

        #region DeleteSubscription method tests
        [Test]
        public void DeleteSubscription_ShouldThrowArgumentNullException_WhenSubscriptionIsNull()
        {
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => subscriptionController.DeleteSubscription(null));
        }

        [Test]
        public void DeleteSubscriptionType_ShouldCallDeleteSubscriptionDataService_WhenSubscriptionExists()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateSubscriptionReader(new [] { subscription }));

            mockDataService.Setup(ds => ds.DeleteSubscription(It.IsAny<int>())).Verifiable();
            
            //Act
            subscriptionController.DeleteSubscription(subscription);

            //Assert
            mockDataService.Verify(ds => ds.DeleteSubscription(It.IsAny<int>()), Times.Once);
        }

        [Test]
        public void DeleteSubscriptionType_ShouldNotCallDeleteSubscriptionDataService_WhenSubscriptionDoesNotExist()
        {
            // Arrange
            var subscription = new SubscriptionBuilder()
                .Build();

            mockDataService.Setup(ds => ds.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                It.IsAny<int>(),
                It.IsAny<int>())).Returns(SubscriptionDataReaderMockHelper.CreateEmptySubscriptionReader());

            //Act
            subscriptionController.DeleteSubscription(subscription);

            //Assert
            mockDataService.Verify(ds => ds.DeleteSubscription(It.IsAny<int>()), Times.Never);
        }
        #endregion
    }
}
