#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Subscriptions.Controllers;
using DotNetNuke.Services.Subscriptions.Entities;
using DotNetNuke.Services.Subscriptions.Data;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Subscriptions.Tests
{
    [TestFixture]
    public class SubscriptionControllerTests
    {
        #region Constants

        private const string SubscriptionName = "Questions";
        private const string FriendlyName = "Questions";
        private const int SubscriptionTypeId = 1;
        private const int DesktopModuleId = 22;

        #endregion

        #region Private Properties

        private Mock<IDataService> _mockDataService;

        private Mock<DataProvider> _dataProvider;
        private Mock<CachingProvider> _cachingProvider;

        #endregion

        #region Setup

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();

            _mockDataService = new Mock<IDataService>();
            _dataProvider = MockComponentProvider.CreateDataProvider();
            _cachingProvider = MockComponentProvider.CreateDataCacheProvider();

            DataService.RegisterInstance(_mockDataService.Object);

            SetupDataProvider();
            SetupDataTables();
        }

        private void SetupDataProvider()
        {
            _dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
        }

        private void SetupDataTables()
        {
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void SubscriptionControllerImpl_Constructor_Throws_On_Null_Input()
        {
            Assert.Throws<ArgumentNullException>(() => new SubscriptionController(null));
        }

        #endregion

        #region ISubscriptionController Tests

        [Test]
        public void SubscriptionController_AddSubscription_Instant_Calls_DataService_AddSubscription()
        {
            // Arrange
            _mockDataService
                .Setup(ds => ds.AddSubscription(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(0)
                .Verifiable();

            // Act
            var controller = new SubscriptionController(_mockDataService.Object);
            var subscriptionId = controller.Subscribe(GetInstantSubscriber());

            // Assert
            _mockDataService.Verify();
        }

        [Test]
        public void SubscriptionController_AddSubscription_Digest_Calls_DataService_AddSubscription()
        {
            // Arrange
            _mockDataService
                .Setup(ds => ds.AddSubscription(
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()))
                .Returns(0)
                .Verifiable();

            // Act
            var controller = new SubscriptionController(_mockDataService.Object);
            var subscriptionId = controller.Subscribe(GetDigestSubscriber());

            // Assert
            _mockDataService.Verify();
        }
        #endregion

        #region Private methods

        private static Subscriber GetInstantSubscriber()
        {
            return new Subscriber
                {
                    ContentItemId = Null.NullInteger,
                    CreatedOnDate = DateTime.UtcNow,
                    Frequency = Frequency.Instant,
                    LastSentOnDate = DateTime.MinValue,
                    ObjectKey = null,
                    PortalId = 0,
                    SubscriberId = Null.NullInteger,
                    SubscriptionTypeId = 1,
                    UserId = 1
                };
        }

        private static Subscriber GetDigestSubscriber()
        {
            return new Subscriber
            {
                ContentItemId = 1,
                CreatedOnDate = DateTime.UtcNow,
                Frequency = Frequency.Daily,
                LastSentOnDate = DateTime.MinValue,
                ObjectKey = "MyTag",
                PortalId = 0,
                SubscriberId = Null.NullInteger,
                SubscriptionTypeId = 2,
                UserId = 1
            };
        }

        #endregion
    }
}