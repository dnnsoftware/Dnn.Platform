using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Roles.Components;
using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Prompt.Commands;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class AddRolesUnitTests : CommandTests
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUsersController> _usersControllerMock;
        private Mock<IRolesController> _rolesControllerMock;

        [SetUp]
        public void RunBeforeEveryTest()
        {
            _userValidatorMock = new Mock<IUserValidator>();
            _usersControllerMock = new Mock<IUsersController>();
            _rolesControllerMock = new Mock<IRolesController>();
            Reset();
        }

        [TestCase]
        public void Run_AddRolesWithValidArguments_ReturnSuccessResponse()
        {
            // Arrange
            var userId = 2;
            var userInfo = GetUser(userId, true);

            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);

            var userInfoList = new List<UserRoleInfo>(
                    new[]
                    {
                        new UserRoleInfo
                        {
                            RoleID = 1,
                            PortalID = testPortalId,
                            IsPublic = true
                        }
                    }
                );

            var total = 1;
            _usersControllerMock
                .Setup(u => u.GetUserRoles(userInfo, "", out total, -1, -1))
                .Returns(userInfoList);

            var rolesList = new List<RoleInfo>(
                    new[]
                    {
                        new RoleInfo
                        {
                            RoleID = 1,
                            RoleName = "Tester"
                        }
                    }
                );

            _rolesControllerMock
                .Setup(r => r.GetRolesByNames(portalSettings, It.IsAny<int>(), It.IsAny<IList<string>>()))
                .Returns(rolesList);

            // Act
            var result = SetupCommandForAddRolesAndRun(userId.ToString(), "--roles", "Tester");

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [TestCase]
        public void Run_AddRolesWhenUserNotValid_ReturnErrorResponse()
        {
            // Arrange
            var userId = 2;
            UserInfo userInfo = null;

            errorResultModel = new ConsoleErrorResultModel("Invalid userId");

            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);

            // Act
            var result = SetupCommandForAddRolesAndRun(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [TestCase]
        public void Run_AddRolesWhenRoleNotValid_ThrowsException()
        {
            // Arrange
            var userId = 2;
            var userInfo = GetUser(userId, true);

            _userValidatorMock
                .Setup(u => u.ValidateUser(userId, portalSettings, null, out userInfo))
                .Returns(errorResultModel);

            var userInfoList = new List<UserRoleInfo>(
                    new[]
                    {
                        new UserRoleInfo
                        {
                            RoleID = 1,
                            PortalID = testPortalId,
                            IsPublic = true
                        }
                    }
                );

            var total = 1;
            _usersControllerMock
                .Setup(u => u.GetUserRoles(userInfo, "", out total, -1, -1))
                .Returns(userInfoList);

            var rolesList = new List<RoleInfo>(
                    new[]
                    {
                        new RoleInfo
                        {
                            RoleID = 1,
                            RoleName = "Tester"
                        }
                    }
                );

            _rolesControllerMock
                .Setup(r => r.GetRolesByNames(portalSettings, -1, It.IsAny<IList<string>>()))
                .Returns(rolesList);

            // Act
            TestDelegate ex = () =>
            SetupCommandForAddRolesAndRun(userId.ToString(), "--roles", "Not Tester");

            // Assert
            Assert.Throws<Exception>(ex, "Should throw exception");
        }

        protected override ConsoleCommandBase DependencyInitializer()
        {
            return new AddRoles(_userValidatorMock.Object, _usersControllerMock.Object, _rolesControllerMock.Object);
        }

        private ConsoleResultModel SetupCommandForAddRolesAndRun(params string[] args)
        {
            args = new[] { "add-roles" }.Concat(args).ToArray();
            command = SetupCommand(args);
            return command.Run();
        }
    }
}
