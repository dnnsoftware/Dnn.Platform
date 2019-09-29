using Moq;
using NUnit.Framework;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Commands;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class DeleteUserUnitTests : CommandTests<DeleteUser>
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;

        protected override string CommandName => "Delete-User";

        protected override DeleteUser CreateCommand()
        {
            return new DeleteUser(_userValidatorMock.Object, _userControllerWrapperMock.Object);
        }

        protected override void ChildSetup()
        {
            _userValidatorMock = new Mock<IUserValidator>();
            _userControllerWrapperMock = new Mock<IUserControllerWrapper>();
        }

        [Test]
        public void Run_DeleteValidUserId_ReturnSuccessResponse()
        {
            // Arrange          
            int userId = 2;

            UserInfo userInfo = GetUser(userId, false);

            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);
            _userValidatorMock
                .Setup(u => u.ValidateUser(-1, portalSettings, null, out userInfo))
                .Returns(errorResultModel);

            _userControllerWrapperMock
                .Setup(u => u.DeleteUserAndClearCache(ref userInfo, false, false))
                .Returns(true);
            _userControllerWrapperMock
                .Setup(u => u.GetUserById(testPortalId, userId))
                .Returns(userInfo);

            // Act
            var result = RunCommand(userId.ToString());

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()
        {
            // Arrange          
            int userId = 2;

            UserInfo userInfo = GetUser(userId, true);

            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);

            // Act
            var result = RunCommand(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_DeleteUserFailed_ReturnErrorResponse()
        {
            // Arrange          
            int userId = 2;

            UserInfo userInfo = GetUser(userId, false);

            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);
            _userValidatorMock
                .Setup(u => u.ValidateUser(-1, portalSettings, null, out userInfo))
                .Returns(errorResultModel);

            _userControllerWrapperMock
                .Setup(u => u.DeleteUserAndClearCache(ref userInfo, false, false))
                .Returns(false);

            // Act
            var result = RunCommand(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_DeleteNullUserId_ReturnErrorResponse()
        {
            // Arrange        
            UserInfo userinfo;
            errorResultModel = new ConsoleErrorResultModel();
            _userValidatorMock
                .Setup(u => u.ValidateUser(-1, portalSettings, null, out userinfo))
                .Returns(errorResultModel);

            // Act
            var result = RunCommand();

            // Assert
            Assert.IsTrue(result.IsError);
        }
    }
}
