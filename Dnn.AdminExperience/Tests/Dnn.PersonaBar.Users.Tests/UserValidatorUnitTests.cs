// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Net;

    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Users.Components;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class UserValidatorUnitTests
    {
        private Mock<IPortalController> _portalControllerMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;
        private Mock<IContentVerifier> _contentVerifierMock;

        private UserValidator _userValidator;

        [SetUp]
        public void RunBeforeEachTest()
        {
            this._portalControllerMock = new Mock<IPortalController>();
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
            this._contentVerifierMock = new Mock<IContentVerifier>();

            this._userValidator = new UserValidator(
                        this._portalControllerMock.Object,
                        this._userControllerWrapperMock.Object,
                        this._contentVerifierMock.Object);
        }

        [Test]
        public void ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse()
        {
            // Arrange
            int? userId = 1;
            var userInfo = this.GetUserInfoWithProfile(userId.Value);
            this.SetupUserControllerWrapperMock(userInfo);

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse()
        {
            // Arrange
            int? userId = 1;
            this.SetupForSiteGroup(true, userId.Value);
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse()
        {
            // Arrange
            int? userId = 1;
            this.SetupForSiteGroup(false, userId.Value);
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void ValidateUser_IfUserIdNotFound_ThenErrorResponse()
        {
            // Arrange
            int? userId = 1;
            UserInfo userInfo = null;
            this.SetupUserControllerWrapperMock(userInfo);

            ArrayList portals = new ArrayList();
            this._portalControllerMock.Setup(p => p.GetPortals()).Returns(portals);

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void ValidateUser_IfUserIdWithoutValue_ThenErrorResponse()
        {
            // Arrange
            int? userId = null;
            UserInfo userInfo;

            // Act
            var result = this._userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsTrue(result.IsError);
        }

        private void SetupUserControllerWrapperMock(UserInfo userInfo)
        {
            KeyValuePair<HttpStatusCode, string> response;

            this._userControllerWrapperMock
                .Setup(
                    u => u.GetUser(
                        It.IsAny<int>(),
                        It.IsAny<PortalSettings>(),
                        It.IsAny<UserInfo>(),
                        out response))
                .Returns(userInfo);
        }

        private void SetupForSiteGroup(bool isAllowed, int userId)
        {
            var userInfo = this.GetUserInfoWithProfile(userId);

            var otherPortalId = 2;
            var portals = new ArrayList();
            portals.Add(new PortalInfo() { PortalID = otherPortalId });
            this._portalControllerMock.Setup(p => p.GetPortals()).Returns(portals);

            this._userControllerWrapperMock.Setup(u => u.GetUserById(It.IsAny<int>(), userId)).Returns(userInfo);
            this._contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(It.IsAny<int>(), It.IsAny<PortalSettings>(), true)).Returns(isAllowed);
        }

        private UserInfo GetUserInfoWithProfile(int userId)
        {
            var userInfo = new UserInfo();
            var profile = new UserProfile();
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
            return userInfo;
        }
    }
}
