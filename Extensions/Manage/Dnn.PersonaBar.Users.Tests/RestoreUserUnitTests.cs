using Moq;
using NUnit.Framework;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Recyclebin.Components;
using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class RestoreUserUnitTests : CommandTests<RestoreUser>
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IRecyclebinController> _recyclebinControllerMock;

        protected override string CommandName { get { return "Restore-User"; } }

        protected override RestoreUser CreateCommand()
        {
            return new RestoreUser(_userValidatorMock.Object, _recyclebinControllerMock.Object);
        }

        [SetUp]
        protected override void ChildSetup()
        {
            _userValidatorMock = new Mock<IUserValidator>();
            _recyclebinControllerMock = new Mock<IRecyclebinController>();
        }

        [Test]
        public void Run_RestoreValidUserId_ReturnSuccessResponse()
        {
            // Arrange    
            var userId = 2;
            UserInfo userInfo = GetUser(userId, true);
            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);
            var message = string.Empty;
            _recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(true);

            // Act
            var result = RunCommand(userId.ToString());

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
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);
            var message = string.Empty;
            _recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(false);

            // Act
            var result = RunCommand(userId.ToString());

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
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userinfo))
                .Returns(errorResultModel);

            // Act
            var result = RunCommand(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_RestoreNullUserId_ReturnErrorResponse()
        {
            // Arrange        
            errorResultModel = new ConsoleErrorResultModel();

            UserInfo userinfo;
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
