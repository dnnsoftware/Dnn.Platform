#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Social.Subscriptions;
using DotNetNuke.Services.Social.Subscriptions.Data;
using DotNetNuke.Tests.Core.Controllers.Messaging.Builders;
using DotNetNuke.Tests.Core.Controllers.Messaging.Mocks;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    [TestFixture]
    public class SubscriptionTypeControllerTests
    {
        private SubscriptionTypeController subscriptionTypeController;
        private Mock<IDataService> mockDataService;
        private Mock<CachingProvider> mockCacheProvider;

        private const string SubscriptionTypesCacheKey = "DNN_" + DataCache.SubscriptionTypesCacheKey;
        
        [SetUp]
        public void SetUp()
        {
            // Setup Mocks and Stub
            mockDataService = new Mock<IDataService>();
            mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();

            DataService.SetTestableInstance(mockDataService.Object);
            
            // Setup SUT
            subscriptionTypeController = new SubscriptionTypeController();
        }

        #region GetSubscriptionTypes method tests
        [Test]
        [Ignore]
        public void GetSubscriptionTypes_ShouldCallDataService_WhenNoError()
        {
            // Arrange
            mockDataService
                .Setup(ds => ds.GetSubscriptionTypes())
                .Returns(SubscriptionTypeDataReaderMockHelper.CreateEmptySubscriptionTypeReader())
                .Verifiable();
            
            //Act
            subscriptionTypeController.GetSubscriptionTypes();

            //Assert
            mockDataService.Verify(ds => ds.GetSubscriptionTypes(), Times.Once());
        }

        [Test]
        public void GetSubscriptionTypes_ShouldThrowArgumentNullException_WhenPredicateIsNull()
        {
            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => subscriptionTypeController.GetSubscriptionTypes(null));
        }
        #endregion

        #region GetSubscriptionType method tests
        [Test]
        public void GetSubscriptionType_ShouldThrowArgumentNullException_WhenPredicateIsNull()
        {
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => subscriptionTypeController.GetSubscriptionType(null));
        }
        #endregion

        #region AddSubscriptionType method tests
        [Test]
        public void AddSubscriptionType_ShouldThrowArgumentNullException_WhenSubscriptionTypeIsNull()
        {
            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => subscriptionTypeController.AddSubscriptionType(null));
        }

        [Test]
        public void AddSubscriptionType_ShouldFilledUpTheSubscriptionTypeIdPropertyOfTheInputSubscriptionTypeEntity_WhenNoError()
        {
            // Arrange
            const int expectedSubscriptionTypeId = 12;
            var subscriptionType = new SubscriptionTypeBuilder().Build();

            mockDataService
                .Setup(ds => ds.AddSubscriptionType(
                    subscriptionType.SubscriptionName, 
                    subscriptionType.FriendlyName, 
                    subscriptionType.DesktopModuleId))
                .Returns(expectedSubscriptionTypeId);

            //Act
            subscriptionTypeController.AddSubscriptionType(subscriptionType);

            //Assert
            Assert.AreEqual(expectedSubscriptionTypeId, subscriptionType.SubscriptionTypeId);
        }

        [Test]
        public void AddSubscriptionType_ShouldCleanCache_WhenNoError()
        {
            // Arrange
            mockDataService.Setup(ds => ds.AddSubscriptionType(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            mockCacheProvider.Setup(cp => cp.Remove(SubscriptionTypesCacheKey)).Verifiable();

            var subscriptionType = new SubscriptionTypeBuilder().Build();

            //Act
            subscriptionTypeController.AddSubscriptionType(subscriptionType);

            //Assert
            mockCacheProvider.Verify(cp => cp.Remove(SubscriptionTypesCacheKey), Times.Once());
        }
        #endregion

        #region DeleteSubscriptionType method tests
        [Test]
        public void DeleteSubscriptionType_ShouldThrowArgumentOutOfRangeException_WhenSubscriptionTypeIdIsNegative()
        {
            // Arrange
            var subscriptionType = new SubscriptionTypeBuilder()
                .WithSubscriptionTypeId(-1)
                .Build();

            // Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => subscriptionTypeController.DeleteSubscriptionType(subscriptionType));
        }

        [Test]
        public void DeleteSubscriptionType_ShouldThrowNullArgumentException_WhenSubscriptionTypeIsNull()
        {
            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => subscriptionTypeController.DeleteSubscriptionType(null));
        }

        [Test]
        public void DeleteSubscriptionType_ShouldCallDataService_WhenNoError()
        {
            // Arrange 
            var subscriptionType = new SubscriptionTypeBuilder().Build();

            mockDataService
                .Setup(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId))
                .Verifiable();
            
            //Act
            subscriptionTypeController.DeleteSubscriptionType(subscriptionType);

            //Assert
            mockDataService.Verify(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId), Times.Once());
        }

        [Test]
        public void DeleteSubscriptionType_ShouldCleanCache_WhenNoError()
        {
            // Arrange
            var subscriptionType = new SubscriptionTypeBuilder().Build();

            mockDataService.Setup(ds => ds.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId));
            mockCacheProvider.Setup(cp => cp.Remove(SubscriptionTypesCacheKey)).Verifiable();
            
            //Act
            subscriptionTypeController.DeleteSubscriptionType(subscriptionType);

            //Assert
            mockCacheProvider.Verify(cp => cp.Remove(SubscriptionTypesCacheKey), Times.Once());
        }
        #endregion

        [TearDown]
        public void TearDown()
        {
            DataService.ClearInstance();
            MockComponentProvider.ResetContainer();
        }
    }
}
