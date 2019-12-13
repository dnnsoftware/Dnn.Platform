// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Data;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Services.Social.Messaging.Data;
using DotNetNuke.Tests.Core.Controllers.Messaging.Builders;
using DotNetNuke.Tests.Core.Controllers.Messaging.Helpers;
using DotNetNuke.Tests.Core.Controllers.Messaging.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    [TestFixture]
    public class UserPreferencesControllerTests
    {
        private UserPreferencesController userPrefencesController;
        private Mock<IDataService> mockDataService;
        private Mock<CachingProvider> mockCacheProvider;

        [SetUp]
        public void SetUp()
        {
            // Setup Mocks and Stub
            mockDataService = new Mock<IDataService>();
            mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();

            DataService.RegisterInstance(mockDataService.Object);
            SetupCachingProviderHelper.SetupCachingProvider(mockCacheProvider);

            // Setup SUT
            userPrefencesController = new UserPreferencesController();
        }

        [TearDown]
        public void TearDown()
        {
            ComponentFactory.Container = null;
        }

        #region Constructor tests
        [Test]
        public void UserPreferencesController_ShouldThrowArgumentNullException_WhenNullDataServiceIsPassedInTheConstructor()
        {
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => new UserPreferencesController(null));
        }
        #endregion

        #region SetUserPreference tests
        [Test]
        public void SetUserPreference_ShouldCallDataService_WhenNoError()
        {
            // Arrange
            var userPreference = new UserPreferenceBuilder().Build();

            mockDataService.Setup(ds => ds.SetUserPreference(
                userPreference.PortalId, 
                userPreference.UserId, 
                (int) userPreference.MessagesEmailFrequency,
                (int)userPreference.NotificationsEmailFrequency)).Verifiable();

            //Act
            userPrefencesController.SetUserPreference(userPreference);

            // Assert
            mockDataService.Verify(ds => ds.SetUserPreference(
                userPreference.PortalId,
                userPreference.UserId,
                (int)userPreference.MessagesEmailFrequency,
                (int)userPreference.NotificationsEmailFrequency), Times.Once);
        }
        #endregion

        #region GetUserPreference tests
        [Test]
        public void GetUserPreference_ShouldReturnNullObject_WhenUserDoesNotHavePreference()
        {
            // Arrange
            var user = GetValidUser();
            mockDataService.Setup(ds => ds.GetUserPreference(
                Constants.PORTAL_ValidPortalId,
                Constants.UserID_User12)).Returns(UserPreferenceDataReaderMockHelper.CreateEmptyUserPreferenceReader);

            //Act
            var userPreference = userPrefencesController.GetUserPreference(user);

            // Assert
            Assert.IsNull(userPreference);
        }

        [Test]
        public void GetUserPreference_ShouldReturnUserPreference_WhenUserHasPreference()
        {
            // Arrange
            var expectedUserPreference = new UserPreferenceBuilder()
                .WithUserId(Constants.UserID_User12)
                .WithPortalId(Constants.PORTAL_ValidPortalId)
                .Build();

            var user = GetValidUser();
            mockDataService.Setup(ds => ds.GetUserPreference(Constants.PORTAL_ValidPortalId,Constants.UserID_User12))
                .Returns(UserPreferenceDataReaderMockHelper.CreateUserPreferenceReader(expectedUserPreference));

            //Act
            var userPreference = userPrefencesController.GetUserPreference(user);

            // Assert
            Assert.IsNotNull(userPreference);
            Assert.AreEqual(expectedUserPreference.MessagesEmailFrequency, userPreference.MessagesEmailFrequency);
            Assert.AreEqual(expectedUserPreference.NotificationsEmailFrequency, userPreference.NotificationsEmailFrequency);
            Assert.AreEqual(user.PortalID, userPreference.PortalId);
            Assert.AreEqual(user.UserID, userPreference.UserId);
        }
        #endregion

        private static UserInfo GetValidUser()
        {
            return new UserInfo
            {
                DisplayName = Constants.UserDisplayName_User12,
                UserID = Constants.UserID_User12,
                PortalID = Constants.PORTAL_ValidPortalId
            };
        }
    }
}
