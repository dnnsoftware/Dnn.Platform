#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Services.Cache;
using DotNetNuke.Subscriptions.Components.Controllers.Internal;
using DotNetNuke.Subscriptions.Components.Entities;
using DotNetNuke.Subscriptions.Providers.Data;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;
using NUnit.Framework;

namespace DotNetNuke.Subscriptions.Tests
{
    [TestFixture]
    public class PublishControllerTests
    {
        #region Constants

        #endregion

        #region Private Properties

        private Mock<IDataService> _mockDataService;
        private Mock<DataProvider> _dataProvider;
        private Mock<CachingProvider> _cachingProvider;

        private Mock<PublishControllerImpl> _mockPublishController;

        #endregion

        #region Setup

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();

            _mockDataService = new Mock<IDataService>();
            _dataProvider = MockComponentProvider.CreateDataProvider();
            _cachingProvider = MockComponentProvider.CreateDataCacheProvider();

            _mockPublishController = new Mock<PublishControllerImpl>(_mockDataService.Object);

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
        public void PublishControllerImpl_Default_Constructor_Does_Not_Throw()
        {
            Assert.DoesNotThrow(() => new PublishControllerImpl());
        }

        #endregion

        #region IPublishController Tests

        #endregion

        #region Private methods

        private static InstantNotification GetInstantNotificationSample()
        {
            return new InstantNotification
                {
                    QueueItem = new QueueItem
                        {
                            QueueId = 1
                        },
                    Subscribers = new List<Subscriber>()
                };
        }

        #endregion
    }
}