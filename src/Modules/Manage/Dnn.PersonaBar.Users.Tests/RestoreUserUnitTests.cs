using Moq;
using NUnit.Framework;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Recyclebin.Components;
using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class RestoreUserUnitTests
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IRecyclebinController> _recyclebinControllerMock;

        private PortalSettings _portalSettings;
        private ConsoleErrorResultModel _errorResultModel;

        private int _testPortalId = 0;

        [SetUp]
        public void RunBeforeEveryTest()
        {
            _userValidatorMock = new Mock<IUserValidator>();
            _recyclebinControllerMock = new Mock<IRecyclebinController>();

            _portalSettings = new PortalSettings();
            _portalSettings.PortalId = _testPortalId;
            _errorResultModel = null;
        }

        [Test]
        public void Run_RestoreValidUserId_ReturnSuccessResponse()
        {
            // Arrange    
            var userId = 2;
            UserInfo userInfo = GetUser(userId, true);
            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, _portalSettings, null, out userInfo))
                .Returns(_errorResultModel);
            var message = string.Empty;
            _recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(true);

            var command = SetupCommand(userId.ToString());

            // Act
            var result = command.Run();

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_RecycleBinControllerRestoringError_ReturnErrorResponse()
        {
            // Arrange        

            var userId = 2;
            UserInfo userInfo = GetUser(userId, true);
            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, _portalSettings, null, out userInfo))
                .Returns(_errorResultModel);
            var message = string.Empty;
            _recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(false);

            var command = SetupCommand(userId.ToString());

            // Act
            var result = command.Run();

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_RestoreNotDeletedUser_ReturnErrorResponse()
        {
            // Arrange        

            var userId = 2;
            UserInfo userinfo = GetUser(userId, false);
            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, _portalSettings, null, out userinfo))
                .Returns(_errorResultModel);

            var command = SetupCommand(userId.ToString());

            // Act
            var result = command.Run();

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_RestoreNullUserId_ReturnErrorResponse()
        {
            // Arrange        
            _errorResultModel = new ConsoleErrorResultModel();

            UserInfo userinfo;
            _userValidatorMock
                .Setup(u => u.ValidateUser(-1, _portalSettings, null, out userinfo))
                .Returns(_errorResultModel);

            var command = SetupCommand(string.Empty);

            // Act
            var result = command.Run();

            // Assert
            Assert.IsTrue(result.IsError);
        }

        private RestoreUser SetupCommand(string userId)
        {
            var command = new RestoreUser(_userValidatorMock.Object, _recyclebinControllerMock.Object);
            var args = new[] { "restore-user", userId };
            command.Initialize(args, _portalSettings, null, -1);
            return command;
        }

        private UserInfo GetUser(int userId, bool isDeleted)
        {
            UserInfo userInfo = new UserInfo();
            var profile = new UserProfile();
            profile.FirstName = "testUser";
            userInfo.UserID = userId;
            userInfo.Profile = profile;
            userInfo.IsDeleted = isDeleted;
            userInfo.PortalID = _testPortalId;
            return userInfo;
        }
    }
}
