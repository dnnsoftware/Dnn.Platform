using System.Collections;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Users.Components;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using System.Net;

namespace Dnn.PersonaBar.Users.Tests
{
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
            _portalControllerMock = new Mock<IPortalController>();
            _userControllerWrapperMock = new Mock<IUserControllerWrapper>();
            _contentVerifierMock = new Mock<IContentVerifier>();

            _userValidator = new UserValidator(
                        _portalControllerMock.Object,
                        _userControllerWrapperMock.Object,
                        _contentVerifierMock.Object
                    );
        }

        [Test]
        public void ValidateUser_IfUserIdWithValidValue_ThenSuccessResponse()
        {
            // Arrange
            int? userId = 1;
            var userInfo = GetUserInfoWithProfile(userId.Value);
            SetupUserControllerWrapperMock(userInfo);

            // Act
            var result = _userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void ValidateUser_IfUserAllowedInSiteGroup_ThenSuccessResponse()
        {
            // Arrange
            int? userId = 1;
            SetupForSiteGroup(true, userId.Value);
            UserInfo userInfo;

            // Act
            var result = _userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public void ValidateUser_IfUserNotAllowedInSiteGroup_ThenErrorResponse()
        {
            // Arrange
            int? userId = 1;
            SetupForSiteGroup(false, userId.Value);
            UserInfo userInfo;

            // Act
            var result = _userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void ValidateUser_IfUserIdNotFound_ThenErrorResponse()
        {
            // Arrange
            int? userId = 1;
            UserInfo userInfo = null;
            SetupUserControllerWrapperMock(userInfo);

            ArrayList portals = new ArrayList();
            _portalControllerMock.Setup(p => p.GetPortals()).Returns(portals);

            // Act
            var result = _userValidator.ValidateUser(userId, null, null, out userInfo);

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
            var result = _userValidator.ValidateUser(userId, null, null, out userInfo);

            // Assert
            Assert.IsTrue(result.IsError);
        }

        private void SetupUserControllerWrapperMock(UserInfo userInfo)
        {
            KeyValuePair<HttpStatusCode, string> response;

            _userControllerWrapperMock
                .Setup(
                    u => u.GetUser(It.IsAny<int>(),
                    It.IsAny<PortalSettings>(),
                    It.IsAny<UserInfo>(),
                    out response)
                )
                .Returns(userInfo);
        }

        private void SetupForSiteGroup(bool isAllowed, int userId)
        {
            var userInfo = GetUserInfoWithProfile(userId);

            var otherPortalId = 2;
            var portals = new ArrayList();
            portals.Add(new PortalInfo() { PortalID = otherPortalId });
            _portalControllerMock.Setup(p => p.GetPortals()).Returns(portals);

            _userControllerWrapperMock.Setup(u => u.GetUserById(It.IsAny<int>(), userId)).Returns(userInfo);
            _contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(It.IsAny<int>(), It.IsAny<PortalSettings>(), true)).Returns(isAllowed);
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