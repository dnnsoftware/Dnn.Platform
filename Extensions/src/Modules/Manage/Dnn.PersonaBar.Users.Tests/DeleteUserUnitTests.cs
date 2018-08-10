using Moq;
using NUnit.Framework;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Prompt.Commands;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class DeleteUserUnitTests
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;

        private PortalSettings _portalSettings;
        private ConsoleErrorResultModel _errorResultModel;        

        private int _testPortalId = 0;

        [SetUp]
        public void RunBeforeEveryTest()
        {
            _userValidatorMock = new Mock<IUserValidator>();
            _userControllerWrapperMock = new Mock<IUserControllerWrapper>();

            _portalSettings = new PortalSettings();
            _portalSettings.PortalId = _testPortalId;
            _errorResultModel = null;
        }

        [Test]
        public void Run_DeleteValidUserId_ReturnSuccessResponse()
        {
            // Arrange          
            int userId = 2;            

            UserInfo userInfo = GetUser(userId, false);

            _userValidatorMock.Setup(u => u.ValidateUser(userId, _portalSettings, null, out userInfo)).Returns(_errorResultModel);
            _userValidatorMock
                .Setup(u => u.ValidateUser(-1, _portalSettings, null, out userInfo))
                .Returns(_errorResultModel);
            _userControllerWrapperMock.Setup(u => u.DeleteUserAndClearCache(ref userInfo, false, false)).Returns(true);
            _userControllerWrapperMock.Setup(u => u.GetUserById(_testPortalId, userId)).Returns(userInfo);

            var command = SetupCommand(userId.ToString());

            // Act
            var result = command.Run();

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()
        {
            // Arrange          
            int userId = 2;
           
            UserInfo userInfo = GetUser(userId, true);

            _userValidatorMock.Setup(u => u.ValidateUser(userId, _portalSettings, null, out userInfo)).Returns(_errorResultModel);

            var command = SetupCommand(userId.ToString());

            // Act
            var result = command.Run();

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_DeleteUserFailed_ReturnErrorResponse()
        {
            // Arrange          
            int userId = 2;            

            UserInfo userInfo = GetUser(userId, false);

            _userValidatorMock.Setup(u => u.ValidateUser(userId, _portalSettings, null, out userInfo)).Returns(_errorResultModel);
            _userValidatorMock
                .Setup(u => u.ValidateUser(-1, _portalSettings, null, out userInfo))
                .Returns(_errorResultModel);
            _userControllerWrapperMock.Setup(u => u.DeleteUserAndClearCache(ref userInfo, false, false)).Returns(false);
           
            var command = SetupCommand(userId.ToString());

            // Act
            var result = command.Run();

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_DeleteNullUserId_ReturnErrorResponse()
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

        private DeleteUser SetupCommand(string userId)
        {
            var command = new DeleteUser(_userValidatorMock.Object, _userControllerWrapperMock.Object);
            var args = new[] { "delete-user", userId };
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
