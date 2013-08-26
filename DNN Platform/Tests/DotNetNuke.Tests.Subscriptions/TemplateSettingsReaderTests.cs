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
    public class TemplateSettingsReaderTests
    {
        #region Constants

        #endregion

        #region Private Properties

        private Mock<IDataService> _mockDataService;
        private Mock<DataProvider> _dataProvider;
        private Mock<CachingProvider> _cachingProvider;

        private ITemplateSettingsReader _settingsReader;

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

            _settingsReader = TemplateSettingsReader.Instance;

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
        public void TemplateSettingsReader_Can_Construct_Itself()
        {
            Assert.DoesNotThrow(() => new TemplateSettingsReaderImpl());
        }

        #endregion
    }
}