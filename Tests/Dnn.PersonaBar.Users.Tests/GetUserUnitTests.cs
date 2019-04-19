using Moq;
using NUnit.Framework;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Prompt.Commands;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class GetUserUnitTests : CommandTests<GetUser>
    {
        private int _userId;
        private UserInfo _userInfo;

        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;

        protected override string CommandName { get { return "Get-User"; } }

        protected override void ChildSetup()
        {
            _userId = 3;
            _userInfo = GetUser(_userId, false);

            _userValidatorMock = new Mock<IUserValidator>();
            _userControllerWrapperMock = new Mock<IUserControllerWrapper>();
        }

        protected override GetUser CreateCommand()
        {
            return new GetUser(_userValidatorMock.Object, _userControllerWrapperMock.Object);
        }

        [Test]
        public void Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            var recCount = 0;

            _userControllerWrapperMock
                .Setup(c => c.GetUsersByEmail(
                    testPortalId,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    ref recCount,
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(_userId);

            _userValidatorMock
                .Setup(u => u.ValidateUser(_userId, portalSettings, null, out _userInfo))
                .Returns(errorResultModel);

            // Act 
            var result = RunCommand("--email", "user1@g.com");

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [TestCase()]
        public void Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            var recCount = 0;

            _userControllerWrapperMock
                .Setup(c => c.GetUsersByUserName(
                    testPortalId,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    ref recCount,
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(_userId);

            _userValidatorMock
                .Setup(u => u.ValidateUser(_userId, portalSettings, null, out _userInfo))
                .Returns(errorResultModel);

            // Act 
            var result = RunCommand("--username", "user1");

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange            
            _userValidatorMock
             .Setup(u => u.ValidateUser(_userId, portalSettings, null, out _userInfo))
             .Returns(errorResultModel);

            // Act 
            var result = RunCommand(_userId.ToString());

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldErrorResponse()
        {
            // Arrange            
            errorResultModel = new ConsoleErrorResultModel();

            _userValidatorMock
                .Setup(u => u.ValidateUser(_userId, portalSettings, null, out _userInfo))
                .Returns(errorResultModel);

            // Act 
            var result = RunCommand(_userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }
    }
}
