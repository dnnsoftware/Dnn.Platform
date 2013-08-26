#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Services.Cache;
using DotNetNuke.Subscriptions.Components.Controllers.Internal;
using DotNetNuke.Subscriptions.Providers.Data;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;
using NUnit.Framework;

namespace DotNetNuke.Subscriptions.Tests
{
    [TestFixture]
    public class InternalSubscriptionControllerTests
    {
        #region Constants

        #endregion

        #region Private Properties

        private Mock<IDataService> _mockDataService;
        private Mock<DataProvider> _dataProvider;
        private Mock<CachingProvider> _cachingProvider;

        private Mock<InternalSubscriptionControllerImpl> _mockInternalSubscriptionController;

        #endregion

        #region Setup

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();

            _mockDataService = new Mock<IDataService>();
            _dataProvider = MockComponentProvider.CreateDataProvider();
            _cachingProvider = MockComponentProvider.CreateDataCacheProvider();

            _mockInternalSubscriptionController = new Mock<InternalSubscriptionControllerImpl>(_mockDataService.Object);

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
        public void InternalSubscriptionControllerImpl_Default_Constructor_Does_Not_Throw()
        {
            Assert.DoesNotThrow(() => new InternalSubscriptionControllerImpl());
        }

        #endregion
    }
}