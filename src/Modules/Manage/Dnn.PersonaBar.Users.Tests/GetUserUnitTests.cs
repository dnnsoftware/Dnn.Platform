using Moq;
using NUnit.Framework;
using System.Linq;
using Dnn.PersonaBar.Users.Components;
using DotNetNuke.Entities.Portals;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Users.Components.Prompt.Commands;
using DotNetNuke.Entities.Users;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class GetUserUnitTests
    {
        private UserInfo _userInfo;
        private PortalSettings _portalSettings;
        private IConsoleCommand _getUserCommand;
        private ConsoleErrorResultModel _errorResultModel;

        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;

        private int? _userId = 3;
        private int _testPortalId = 1;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            _errorResultModel = null;
            _userValidatorMock = new Mock<IUserValidator>();
            _userControllerWrapperMock = new Mock<IUserControllerWrapper>();

            _userInfo = new UserInfo();
            var profile = new UserProfile();
            profile.FirstName = "testUser";            
            _userInfo.UserID = _userId.Value;
            _userInfo.Profile = profile;

            _portalSettings = new PortalSettings();
            _portalSettings.PortalId = _testPortalId;

            _userValidatorMock.Setup(u => u.ValidateUser(_userId, _portalSettings, null, out _userInfo)).Returns(_errorResultModel);
        }

        [TestCase()]
        public void Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            var recCount = 0;
            var cmd = "--email user1@g.com";

            _userControllerWrapperMock.Setup(c => c.GetUsersByEmail(_testPortalId, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), ref recCount, It.IsAny<bool>(), It.IsAny<bool>())).Returns(_userId);
            
            SetupCommand(cmd.Split(new[] { ' ' }));

            // Act 
            var result = _getUserCommand.Run();

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [TestCase()]
        public void Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            var recCount = 0;
            var cmd = "--username user1";

            _userControllerWrapperMock.Setup(c => c.GetUsersByUserName(_testPortalId, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), ref recCount, It.IsAny<bool>(), It.IsAny<bool>())).Returns(_userId);

            SetupCommand(cmd.Split(new[] { ' ' }));

            // Act 
            var result = _getUserCommand.Run();

            // Assert
            Assert.IsFalse(result.IsError);
        }
        
        [Test]
        public void Run_GetUserWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            SetupCommand(new[] { _userId.Value.ToString() });

            // Act 
            var result = _getUserCommand.Run();

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldErrorResponse()
        {
            // Arrange
            _errorResultModel = new ConsoleErrorResultModel();

            _userValidatorMock.Setup(u => u.ValidateUser(_userId, _portalSettings, null, out _userInfo)).Returns(_errorResultModel);

            SetupCommand(new[] { _userId.Value.ToString() });

            // Act 
            var result = _getUserCommand.Run();

            // Assert
            Assert.IsTrue(result.IsError);
        }       
        
        private void SetupCommand(string[] argParams)
        {
            _getUserCommand = new GetUser(_userValidatorMock.Object, _userControllerWrapperMock.Object);

            var args = argParams.ToList();
            args.Insert(0, "get-user");
            _getUserCommand.Initialize(args.ToArray(), _portalSettings, null, _userId.Value);
        }
    }
}
