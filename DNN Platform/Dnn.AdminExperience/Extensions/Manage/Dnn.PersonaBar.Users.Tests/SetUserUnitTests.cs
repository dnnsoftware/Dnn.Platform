using Moq;
using NUnit.Framework;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Users.Components.Prompt.Commands;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class SetUserUnitTests : CommandTests<SetUser>
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUsersController> _usersControllerMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;

        protected override string CommandName => "Set-User";

        protected override void ChildSetup()
        {
            _userValidatorMock = new Mock<IUserValidator>();
            _usersControllerMock = new Mock<IUsersController>();
            _userControllerWrapperMock = new Mock<IUserControllerWrapper>();
        }

        protected override SetUser CreateCommand()
        {
            return new SetUser(_userValidatorMock.Object, _usersControllerMock.Object, _userControllerWrapperMock.Object);
        }

        [Test]
        public void Run_UserIdNull_ReturnErrorResponse()
        {
            // Arrange          
            UserInfo userInfo;
            errorResultModel = new ConsoleErrorResultModel();
            _userValidatorMock
                .Setup(u => u.ValidateUser(-1, portalSettings, null, out userInfo))
                .Returns(errorResultModel);

            // Act
            var result = RunCommand("--username", "testusername", "--firstname", "testfirstname", "--lastname", "testlastname");

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [TestCase("--email", "user@gm.com")]
        [TestCase("--password", "pass1233")]
        [TestCase("--username", "user4pmt")]
        [TestCase("--displayname", "user4displayname")]
        [TestCase("--approved", "true")]
        public void Run_ValidCommand_ReturnSuccessResponse(string attributeName, string attributeValue)
        {
            // Arrange
            var userId = 4;
            var userInfo = GetUser(userId, false);
            userInfo.FirstName = "userFirstName";
            userInfo.LastName = "userLastName";
            userInfo.Email = "user@email.com";

            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);
            _userControllerWrapperMock
                .Setup(w => w.GetUserById(testPortalId, userId))
                .Returns(userInfo);

            // Act
            var result = RunCommand(userId.ToString(), "--firstname", "user4", "--lastname", "user4", attributeName, attributeValue);

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(1, result.Records);
        }
    }
}