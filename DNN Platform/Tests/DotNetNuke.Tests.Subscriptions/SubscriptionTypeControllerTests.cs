#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
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
    public class SubscriptionTypeControllerTests
    {
        #region Constants

        private const string SubscriptionName = "Questions";
        private const string FriendlyName = "Questions";
        private const int SubscriptionTypeId = 1;
        private const int DesktopModuleId = 22;

        #endregion

        #region Private Properties

        private Mock<IDataService> _mockDataService;

        private Mock<SubscriptionTypeControllerImpl> _mockSubscriptionTypeController;

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

            _mockSubscriptionTypeController = new Mock<SubscriptionTypeControllerImpl>(_mockDataService.Object)
                {CallBase = true};

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
        public void SubscriptionsController_Constructor_Throws_On_Null_Input()
        {
            Assert.Throws<ArgumentNullException>(() => new SubscriptionControllerImpl(null));
        }

        #endregion

        #region Subscription Type Tests

        [Test]
        public void SubscriptionsController_AddSubscriptionType_Throws_On_Null_SubscriptionType()
        {
            Assert.Throws<ArgumentNullException>(() => _mockSubscriptionTypeController.Object.AddSubscriptionType(null));
        }

        [Test]
        public void SubscriptionsController_AddSubscriptionType_Calls_DataService_On_Valid_Data()
        {
            //Arrange            
            _mockDataService
                .Setup(ds => ds.AddSubscriptionType(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Verifiable();

            //Act
            _mockSubscriptionTypeController.Object.AddSubscriptionType(GetSampleSubscriptionType());

            //Assert
            _mockDataService.Verify();
        }

        #endregion

        #region Private Methods

        private static SubscriptionType GetSampleSubscriptionType()
        {
            return new SubscriptionType
                {
                    SubscriptionTypeId = SubscriptionTypeId,
                    SubscriptionName = SubscriptionName,
                    FriendlyName = FriendlyName,
                    DesktopModuleId = DesktopModuleId
                };
        }

        #endregion
    }
}